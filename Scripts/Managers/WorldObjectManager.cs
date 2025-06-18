using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node {
    private const int BlockSpawnDistance = 20;

    private Game _game;
    private Array<WorldObject>[,] _activeWorldObjects;
    private int _currentLoadCellCount;

    private bool _isStartAreaLoading;
    private bool _isWorldLoading;

    private Array<Dictionary>[,] _unspawnedWorldObjects;
    private List<(int x, int y)> _loadingQueue = new();
    private int _worldSpawnThreshold;

    public event Action WorldLoaded;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;

        Player.PlayerSpawned += OnPlayerSpawned;

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

        Array defaultSpawnPos =
            worldData["DefaultSpawnPosition"].AsGodotArray();
        (int x, int y) loadingOrigin =
            ((int)defaultSpawnPos[0], (int)defaultSpawnPos[1]);
        SetLoadingQueue(loadingOrigin);

        Array allWorldObjects =
            worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            _unspawnedWorldObjects[
                (int)dictionary["xPosition"].ToString().ToFloat(),
                (int)dictionary["yPosition"].ToString().ToFloat()
            ].Add(dictionary);
        }

        _isStartAreaLoading = true;
        _isWorldLoading = true;

        TreeExiting += OnExiting;
    }

    private void SetLoadingQueue((int x, int y) loadingOrigin) {
        int distanceFromOrigin = 0;
        _loadingQueue.Add(loadingOrigin);
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
                QueueCellIfValid((topLeft.x + i, topLeft.y));
                QueueCellIfValid((topRight.x, topRight.y + i));
                QueueCellIfValid((bottomRight.x - i, bottomRight.y));
                QueueCellIfValid((bottomLeft.x, bottomLeft.y - i));
            }
        }
    }

    private void QueueCellIfValid((int x, int y) currentCell) {
        if (currentCell.x < 0 || currentCell.x >= _game.Width ||
            currentCell.y < 0 || currentCell.y >= _game.Height) return;
        _loadingQueue.Add(currentCell);
    }

    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerSpawned;
        TreeExiting -= OnExiting;
    }

    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;

        // RPC the host with playerData, 
        RpcId(SceneManager.HostId, nameof(ClientJoinedGame),
            playerData, _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ClientJoinedGame(Dictionary playerData, int clientPeerId) {
        IntVector spawnPosition = new(5, 5);
        List<IntVector> region = _game.Region.GetRegion(
            spawnPosition, BlockSpawnDistance);
        Array localWorldObjects = new();
        foreach (WorldObject worldObject in GetObjectsInRegion(region)) {
            localWorldObjects.Add(worldObject.ToDictionary());
        }

        RpcId(clientPeerId, nameof(SendClientLocalWorldObjects),
            localWorldObjects);
    }

    [Rpc]
    private void SendClientLocalWorldObjects(Array worldObjects) {
        _activeWorldObjects = new Array<WorldObject>[_game.Width, _game.Height];
        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
                _activeWorldObjects[x, y] = new Array<WorldObject>();
            }
        }

    }


    public Array<WorldObject> GetCellContents(IntVector coords) {
        return _activeWorldObjects[coords.X, coords.Y] ?? 
               new Array<WorldObject>();
    }

    public override void _Process(double delta) {
        if (_isWorldLoading) {
            ProcessLoadWorld(16, out bool finished);
            if (finished) {
                _isWorldLoading = false;
            }
        }
    }

    private void ProcessLoadWorld(float timeout, out bool finished) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (
            _currentLoadCellCount < _loadingQueue.Count &&
            stopwatch.ElapsedMilliseconds < timeout
        ) {
            (int x, int y) coords = _loadingQueue[_currentLoadCellCount];
            Array<Dictionary> cellObjects =
                _unspawnedWorldObjects[coords.x, coords.y];
            _activeWorldObjects[coords.x, coords.y] =
                new Array<WorldObject>();
            foreach (Dictionary dictionary in cellObjects) {
                WorldObject worldObject =
                    WorldObject.FromDictionary(dictionary);
                AddWorldObject(worldObject);
            }

            _currentLoadCellCount++;
        }
        

        if (_isStartAreaLoading && 
            _currentLoadCellCount >= _worldSpawnThreshold) {
            _isStartAreaLoading = false;
            WorldLoaded?.Invoke();
        }

        finished = _currentLoadCellCount == _loadingQueue.Count;
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

    private void OnLocalPlayerMoved(Dictionary positionChange) {
        IntVector oldCoordinates = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector newCoordinates = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        List<IntVector> newRegion = _game.Region.GetRegionDelta(
            newCoordinates, oldCoordinates, BlockSpawnDistance);

        EnableObjectsInRegion(newRegion);
    }

    private void OnPlayerSpawned(Player player) {
        List<IntVector> region = _game.Region.GetRegion(
            player.SpawnCoords, BlockSpawnDistance);

        EnableObjectsInRegion(region);
        player.MovedCell += OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted
            += OnPlayerGatherAction;
        player.ActionController.BuildAction.BuildBlockActionAttempted
            += OnPlayerBuildBlockAction;
        player.ActionController.BuildAction.BuildWallActionAttempted
            += OnPlayerBuildWallAction;
        player.PlayerDespawned += OnPlayerDespawned;
        player.Inventory.PickupLooted += OnPlayerPickupLooted;
    }

    private void OnPlayerDespawned(Player player) {
        player.MovedCell -= OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted
            -= OnPlayerGatherAction;
        player.ActionController.BuildAction.BuildBlockActionAttempted
            -= OnPlayerBuildBlockAction;
        player.ActionController.BuildAction.BuildWallActionAttempted
            -= OnPlayerBuildWallAction;

        player.PlayerDespawned -= OnPlayerDespawned;
        player.Inventory.PickupLooted -= OnPlayerPickupLooted;
    }

    private void OnPlayerGatherAction(IntVector coords, Player player) {
        foreach (WorldObject worldObject in GetCellContents(coords)) {
            if (worldObject.TryGetProperty(out ObjectGatherable gatherable)) {
                gatherable.GatherAction(player);
                player.ActionController.GatherAction.OnAfterGatherSuccess();
                return;
            }
        }
    }

    private void OnPlayerBuildBlockAction(
        Player player, Item item, IntVector coords) {
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
        player.Inventory.OnAfterBuildSuccess(item);
    }

    private void OnPlayerBuildWallAction(Player player, Item item, IntVector coords) {
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
        player.Inventory.OnAfterBuildSuccess(item);
    }

    private void OnPlayerPickupLooted(WorldObject worldObject) {
        OnWorldObjectDestroyed(worldObject);
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
        }

        worldObject.QueueFree();
    }
}