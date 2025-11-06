using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node {
    private const int BlockSpawnDistance = 20;

    private Game _game;
    private Array<WorldObject>[,] _activeWorldObjects;
    private string _worldName;

    private Array<Dictionary>[,] _unspawnedWorldObjects;
    private List<(int x, int y)> _loadingQueue;
    private int _worldSpawnThreshold;
    private (int x, int y) _defaultSpawnPosition;
    private WorldObjectLoader _worldObjectLoader;

    private Dictionary _localPlayerData;

    private Godot.Collections.Dictionary<int, Player> _players = new();

    public event Action WorldLoadedLocally;
    public event Action WorldSaved;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;

        _worldSpawnThreshold = (int)Math.Pow(2 * BlockSpawnDistance - 1, 2);
        _activeWorldObjects =
            new Array<WorldObject>[_game.Width, _game.Height];
        _unspawnedWorldObjects =
            new Array<Dictionary>[_game.Width, _game.Height];
        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
                _unspawnedWorldObjects[x, y] = new Array<Dictionary>();
            }
        }

        _worldName = worldData["Name"].ToString();
        Array defaultSpawnPos =
            worldData["DefaultSpawnPosition"].AsGodotArray();
        _defaultSpawnPosition =
            ((int)defaultSpawnPos[0], (int)defaultSpawnPos[1]);
        _loadingQueue = CreateLoadingQueue(_defaultSpawnPosition);

        Array allWorldObjects =
            worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            _unspawnedWorldObjects[
                (int)dictionary["xPosition"].ToString().ToFloat(),
                (int)dictionary["yPosition"].ToString().ToFloat()
            ].Add(dictionary);
        }

        InitializeWorldObjectLoader();

        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    public override void _Ready() {
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnExitGameClicked() {
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        
        Dictionary worldData = new();
        worldData.Add("Name", _worldName);
        worldData.Add("Width", _game.Width);
        worldData.Add("Height", _game.Height);
        worldData.Add("PlayerPositions", new Array());
        worldData.Add("DefaultSpawnPosition",
            new Array { _defaultSpawnPosition.x, _defaultSpawnPosition.y });
        Array savedWorldObjects = new();

        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
                if (_activeWorldObjects[x, y] is null) {
                    foreach (Dictionary worldObjectData in _unspawnedWorldObjects[x, y]) {
                        if (worldObjectData["type"].ToString() == "component") continue;
                        savedWorldObjects.Add(worldObjectData);
                    }
                } else {
                    foreach (WorldObject worldObject in _activeWorldObjects[x, y]) {
                        if (worldObject.Type == "component") continue;
                        savedWorldObjects.Add(worldObject.ToDictionary());
                    }
                }
            }
        }

        worldData.Add("SavedWorldObjects", savedWorldObjects);
        FileManager.SaveWorld(worldData);

        WorldSaved?.Invoke();
    }

    private void OnLocalPlayerSpawned(Player player) {
        // listen to player's actions
        player.ActionController.GatherAction.GatherAttempted +=
            OnLocalPlayerGatherAction;
        player.ActionController.BuildAction.BuildBlockActionAttempted +=
            OnLocalPlayerBuildBlockAction;
        player.ActionController.BuildAction.BuildWallActionAttempted +=
            OnLocalPlayerBuildWallAction;
        player.Inventory.PickupLooted += OnLocalPlayerPickupLooted;

        // when player is deleted, unsubscribe from all events
    }

    private List<(int x, int y)> CreateLoadingQueue((int x, int y) loadingOrigin) {
        List<(int x, int y)> loadingQueue = new();
        int distanceFromOrigin = 0;
        loadingQueue.Add(loadingOrigin);
        while (distanceFromOrigin <= Math.Max(_game.Width, _game.Height)) {
            distanceFromOrigin++;
            (int x, int y) topLeft = (
                loadingOrigin.x - distanceFromOrigin,
                loadingOrigin.y - distanceFromOrigin
            );
            (int x, int y) topRight = (
                loadingOrigin.x + distanceFromOrigin,
                loadingOrigin.y - distanceFromOrigin
            );
            (int x, int y) bottomLeft = (
                loadingOrigin.x - distanceFromOrigin,
                loadingOrigin.y + distanceFromOrigin
            );
            (int x, int y) bottomRight = (
                loadingOrigin.x + distanceFromOrigin,
                loadingOrigin.y + distanceFromOrigin
            );
            for (int i = 0; i <= 2 * distanceFromOrigin - 1; i++) {
                QueueCellIfValid(loadingQueue, (topLeft.x + i, topLeft.y));
                QueueCellIfValid(loadingQueue, (topRight.x, topRight.y + i));
                QueueCellIfValid(loadingQueue, (bottomRight.x - i, bottomRight.y));
                QueueCellIfValid(loadingQueue, (bottomLeft.x, bottomLeft.y - i));
            }
        }

        return loadingQueue;
    }

    private void QueueCellIfValid(
        List<(int x, int y)> loadingQueue, (int x, int y) currentCell
    ) {
        if (currentCell.x < 0 || currentCell.x >= _game.Width ||
            currentCell.y < 0 || currentCell.y >= _game.Height) return;
        loadingQueue.Add(currentCell);
    }


    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;
        // if the playerData contains a defaultSpawnPosition, we use that
        // otherwise, we need to get the information from the server
        // before we spawn the player

        RpcId(SceneManager.HostId, nameof(CmdRequestWorldData),
            _game.PeerId, _defaultSpawnPosition.x, _defaultSpawnPosition.y);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void CmdRequestWorldData(int peerId, int spawnX, int spawnY) {
        List<(int x, int y)> loadingQueueForPeer =
            CreateLoadingQueue((spawnX, spawnY));

        Array worldObjects = new();
        foreach ((int x, int y) cell in loadingQueueForPeer) {
            Dictionary cellInformation = new() {
                { "x", cell.x },
                { "y", cell.y },
            };
            if (_activeWorldObjects[cell.x, cell.y] is null) {
                cellInformation.Add("objects",
                    _unspawnedWorldObjects[cell.x, cell.y]);
            } else {
                Array cellObjects = new();
                foreach (WorldObject worldObject in _activeWorldObjects[cell.x, cell.y]) {
                    cellObjects.Add(worldObject.ToDictionary());
                }

                cellInformation.Add("objects", cellObjects);
            }


            worldObjects.Add(cellInformation);
        }

        RpcId(peerId, nameof(RpcReceiveWorldData),
            worldObjects);
    }

    [Rpc]
    private void RpcReceiveWorldData(Array worldData) {
        //@todo pull game size from worldData
        _worldSpawnThreshold = (int)Math.Pow(2 * BlockSpawnDistance - 1, 2);
        _activeWorldObjects =
            new Array<WorldObject>[_game.Width, _game.Height];
        _unspawnedWorldObjects =
            new Array<Dictionary>[_game.Width, _game.Height];

        _defaultSpawnPosition = (5, 5);
        _loadingQueue = CreateLoadingQueue(_defaultSpawnPosition);

        foreach (Dictionary dictionary in worldData) {
            int x = (int)dictionary["x"].ToString().ToFloat();
            int y = (int)dictionary["y"].ToString().ToFloat();
            _unspawnedWorldObjects[x, y] = new Array<Dictionary>();
            foreach (Dictionary objectDict in
                     dictionary["objects"].AsGodotArray()) {
                _unspawnedWorldObjects[x, y].Add(objectDict);
            }
        }

        InitializeWorldObjectLoader();
    }
    
    private void InitializeWorldObjectLoader() {
        _worldObjectLoader = new WorldObjectLoader();
        _worldObjectLoader.Initialize(_loadingQueue, _unspawnedWorldObjects, _worldSpawnThreshold);
        _worldObjectLoader.OnWorldObjectAdd = AddWorldObject;
        _worldObjectLoader.OnCellLoadStart = (x, y) => {
            _activeWorldObjects[x, y] = new Array<WorldObject>();
        };
        _worldObjectLoader.OnStartAreaLoaded = () => {
            SpawnLocalPlayer();
            WorldLoadedLocally?.Invoke();
        };
        AddChild(_worldObjectLoader);
    }


    private Array<WorldObject> GetCellContents(int x, int y) {
        return _activeWorldObjects[x, y] ??
               new Array<WorldObject>();
    }

    private Array<WorldObject> GetCellContents(IntVector coords) {
        return _activeWorldObjects[coords.X, coords.Y] ??
               new Array<WorldObject>();
    }


    private void SpawnLocalPlayer() {
        // @todo consider making players a type of WorldObject
        Player player = Player.Create(_game.PeerId, new IntVector(5, 5));
        player.InitAsLocal(_game, _localPlayerData);
        _game.PlayerParent.AddChild(player, true);
        _players.Add(_game.PeerId, player);

        Rpc(nameof(RpcOnNewPlayerJoining), _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcOnNewPlayerJoining(int newPeerId) {
        Player player = Player.Create(newPeerId, new IntVector(5, 5));
        _game.PlayerParent.AddChild(player, true);
        _players.Add(newPeerId, player);

        _players[_game.PeerId].AddPeerToSynchronizer(newPeerId);

        RpcId(newPeerId, nameof(SpawnRemoteExistingPlayer), _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SpawnRemoteExistingPlayer(int peerId) {
        Player player = Player.Create(peerId,
            new IntVector(5, 5));
        _game.PlayerParent.AddChild(player, true);
        _players.Add(peerId, player);
    }

    private void EnableObjectsInRegion(List<IntVector> region) {
        Array<WorldObject> objects = GetObjectsInRegion(region);
        foreach (WorldObject worldObject in objects) {
            worldObject.Enable();
        }
    }

    private Array<WorldObject> GetObjectsInRegion(List<IntVector> region) {
        Array<WorldObject> objects = new();
        foreach (IntVector coords in region) {
            Array<WorldObject> cellContents = GetCellContents(coords);
            objects.AddRange(cellContents);
        }

        return objects;
    }

    // Gather
    private void OnLocalPlayerGatherAction(IntVector coords, Player player) {
        RpcId(SceneManager.HostId, nameof(CmdPlayerGatherAction),
            coords.X, coords.Y, player.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CmdPlayerGatherAction(int x, int y, int peerId) {
        Player player = _players[peerId];
        foreach (WorldObject worldObject in GetCellContents(x, y)) {
            if (!worldObject.TryGetProperty(out ObjectGatherable gatherable)) continue;
            gatherable.GatherAction(player);
            RpcId(peerId, nameof(RpcGatherSuccess));
            return;
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcGatherSuccess() {
        Player player = _players[_game.PeerId];
        player.ActionController.GatherAction.OnAfterGatherSuccess();
    }

    private void OnLocalPlayerBuildBlockAction(
        Player player, Item item, IntVector coords) {
        RpcId(SceneManager.HostId,
            nameof(CmdPlayerBuildBlockAction),
            item.ToDictionary(), coords.X, coords.Y, player.PeerId
        );
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CmdPlayerBuildBlockAction(
        Dictionary data, int x, int y, int peerId
    ) {
        Item item = Item.FromDictionary(data);
        IntVector coords = new(x, y);

        if (!item.TryGetProperty(out ItemPlaceable placeable)) return;

        WorldObject block;
        switch (placeable.Type) {
            case PlaceableType.Block:
                foreach (WorldObject worldObject in GetCellContents(coords)) {
                    if (worldObject.TryGetProperty(out ObjectPlacementCollision collision) &&
                        collision.Layer == PlacementCollisionLayer.Foreground) {
                        return;
                    }
                }

                block = WorldObject.New(coords).AsBlock(item).Build();
                break;
            case PlaceableType.Prop:
                List<IntVector> region = placeable.OccupiedCells.Select(
                    intVector => coords + intVector).ToList();

                foreach (WorldObject worldObject in GetObjectsInRegion(region)) {
                    if (worldObject.TryGetProperty(out ObjectPlacementCollision collision) &&
                        collision.Layer == PlacementCollisionLayer.Foreground) {
                        return;
                    }
                }

                block = WorldObject.New(coords).AsProp(item).Build();
                foreach (IntVector regionCoords in region) {
                    WorldObject component = WorldObject.New(regionCoords)
                        .AsComponent(block).Build();
                    AddWorldObject(component);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(placeable.Type), placeable.Type, "Unknown placeable type");
        }

        AddWorldObject(block);
        Rpc(nameof(RpcWorldObjectCreate), block.ToDictionary());
        RpcId(peerId, nameof(RpcBuildSuccess),
            item.ToDictionary());
    }

    [Rpc(CallLocal = true)]
    private void RpcBuildSuccess(Dictionary data) {
        Item item = Item.FromDictionary(data);
        Player player = _players[_game.PeerId];
        player.Inventory.OnAfterBuildSuccess(item);
    }

    [Rpc]
    private void RpcWorldObjectCreate(Dictionary data) {
        WorldObject worldObject = WorldObject.FromDictionary(data);
        AddWorldObject(worldObject);
    }

    [Rpc]
    private void RpcWorldObjectDestroy(Dictionary data) {
        int x = (int)data["xPosition"].ToString().ToFloat();
        int y = (int)data["yPosition"].ToString().ToFloat();
        string type = data["type"].ToString();
        foreach (WorldObject worldObject in _activeWorldObjects[x, y]) {
            if (worldObject.Type != type) continue;
            _activeWorldObjects[x, y].Remove(worldObject);
            worldObject.QueueFree();
            return;
        }

        throw new Exception("[20250621.0018.1] Couldn't find worldObject to destroy on peer");
    }

    private void OnLocalPlayerBuildWallAction(Player player, Item item, IntVector coords) {
        if (!item.TryGetProperty(out ItemPlaceable placeable)) return;
        if (placeable.Type == PlaceableType.Prop) return;

        foreach (WorldObject worldObject in GetCellContents(coords)) {
            if (worldObject.TryGetProperty(out ObjectPlacementCollision collision) &&
                collision.Layer == PlacementCollisionLayer.Background) {
                return;
            }
        }

        WorldObject block = WorldObject.New(coords)
            .AsWall(item)
            .Build();
        AddWorldObject(block);
        Rpc(nameof(RpcWorldObjectCreate), block.ToDictionary());
        player.Inventory.OnAfterBuildSuccess(item);
    }

    private void OnLocalPlayerPickupLooted(WorldObject worldObject) {
        RpcId(SceneManager.HostId, nameof(CmdPlayerPickedLooted),
            worldObject.ToDictionary());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CmdPlayerPickedLooted(Dictionary data) {
        int xPosition = (int)data["xPosition"].ToString().ToFloat();
        int yPosition = (int)data["yPosition"].ToString().ToFloat();
        string type = data["type"].AsString();
        Array<WorldObject> cellContents = GetCellContents(xPosition, yPosition);
        foreach (WorldObject worldObject in cellContents) {
            if (worldObject.Type != type) continue;
            OnWorldObjectDestroyed(worldObject);
            return;
        }
    }

    private void AddWorldObject(WorldObject worldObject) {
        _activeWorldObjects[worldObject.Coords.X, worldObject.Coords.Y]
            .Add(worldObject);
        _game.BlockParent.AddChild(worldObject, true);
        worldObject.Destroyed += OnWorldObjectDestroyed;
        worldObject.Enable();
    }

    private void OnWorldObjectDestroyed(WorldObject worldObject) {
        worldObject.Destroyed -= OnWorldObjectDestroyed;
        _activeWorldObjects[worldObject.Coords.X, worldObject.Coords.Y]
            .Remove(worldObject);
        if (worldObject.TryGetProperty(out ObjectSpawnOnDeath objectDropsPickup)) {
            WorldObject pickup = WorldObject.New(worldObject.Coords)
                .AsPickup(objectDropsPickup.Item).Build();
            AddWorldObject(pickup);
            Rpc(nameof(RpcWorldObjectCreate), pickup.ToDictionary());
        }

        worldObject.QueueFree();
        if (worldObject.Type == "component") return;
        Rpc(nameof(RpcWorldObjectDestroy),
            worldObject.ToDictionary());
    }
}