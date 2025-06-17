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
    private int _currentObjectCount;

    private bool _isStartAreaLoading;
    private bool _isWorldLoading;

    private Array _spawnFirst = new();
    private Array _spawnSecond = new();

    public event Action WorldLoaded;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;

        Player.PlayerSpawned += OnPlayerSpawned;

        _activeWorldObjects = new Array<WorldObject>[_game.Width, _game.Height];
        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
                _activeWorldObjects[x, y] = new Array<WorldObject>();
            }
        }

        IntVector spawnPosition =
            new(worldData["DefaultSpawnPosition"].AsGodotArray());

        Array allWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();

        foreach (Dictionary worldObject in allWorldObjects) {
            int xPosition = (int)worldObject["xPosition"].ToString().ToFloat();
            int yPosition = (int)worldObject["yPosition"].ToString().ToFloat();

            int xDiff = Math.Abs(xPosition - spawnPosition.X);
            int yDiff = Math.Abs(yPosition - spawnPosition.Y);
            if (xDiff <= BlockSpawnDistance && yDiff <= BlockSpawnDistance) {
                _spawnFirst.Add(worldObject);
            } else {
                _spawnSecond.Add(worldObject);
            }
        }
        
        _isStartAreaLoading = true;

        TreeExiting += OnExiting;
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
        
        _spawnFirst = worldObjects;
        _isStartAreaLoading = true;
    }
    
    
    
    public Array<WorldObject> GetCellContents(IntVector coords) {
        return _activeWorldObjects[coords.X, coords.Y];
    }

    public override void _Process(double delta) {
        if (_isStartAreaLoading) {
            ProcessLoadWorld(_spawnFirst, 16, out bool finished);
            if (finished) {
                _isStartAreaLoading = false;
                _isWorldLoading = true;
                WorldLoaded?.Invoke();
            }
        }
        // else if (_isWorldLoading) {
        //     ProcessLoadWorld(_spawnSecond, 16, out bool finished);
        //     if (finished) {
        //         _isWorldLoading = false;
        //     }
        // }
    }

    private void ProcessLoadWorld(Array spawnArray, float timeout, out bool finished) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (_currentObjectCount < spawnArray.Count &&
               stopwatch.ElapsedMilliseconds < timeout) {
            Dictionary savedObjectDict =
                spawnArray[_currentObjectCount].AsGodotDictionary();

            WorldObject worldObject =
                WorldObject.FromDictionary(savedObjectDict);
            AddWorldObject(worldObject);

            _currentObjectCount++;
        }

        if (_currentObjectCount == spawnArray.Count) {
            finished = true;
            _currentObjectCount = 0;
        } else {
            finished = false;
        }
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
        GD.Print(player.Name);
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