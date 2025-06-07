using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node {
    public const int BlockSpawnDistance = 20;

    private Game _game;
    private Array<WorldObject>[,] _activeWorldObjects;
    private int _currentObjectCount;
    private bool _isStartAreaLoaded;
    private bool _isWorldLoaded;

    private Array _spawnFirst = new();
    private Array _spawnSecond = new();

    public event Action WorldLoaded;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }

    public void SetGame(Game game, Dictionary worldData, Dictionary playerData) {
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

        TreeExiting += OnExiting;
    }

    public Array<WorldObject> GetCellContents(int x, int y) {
        return _activeWorldObjects[x, y];
    }

    public override void _Process(double delta) {
        if (!_isStartAreaLoaded) {
            int count = _spawnFirst.Count;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            while (_currentObjectCount < count &&
                   stopwatch.ElapsedMilliseconds < 16) {
                Dictionary savedWorldObjectDict =
                    _spawnFirst[_currentObjectCount].AsGodotDictionary();

                WorldObject newObject = WorldObject.Create(savedWorldObjectDict);
                _game.BlockParent.AddChild(newObject, true);
                _activeWorldObjects[newObject.XPosition, newObject.YPosition]
                    .Add(newObject);

                _currentObjectCount++;
            }

            if (_currentObjectCount < count) return;
            _currentObjectCount = 0;
            _isStartAreaLoaded = true;
            WorldLoaded?.Invoke();
        } else if (!_isWorldLoaded) {
            int count = _spawnSecond.Count;
            Stopwatch stopwatch = new();
            stopwatch.Start();
            while (_currentObjectCount < count &&
                   stopwatch.ElapsedMilliseconds < 16) {
                Dictionary savedWorldObjectDict =
                    _spawnSecond[_currentObjectCount].AsGodotDictionary();

                WorldObject newObject = WorldObject.Create(savedWorldObjectDict);
                _game.BlockParent.AddChild(newObject, true);
                _activeWorldObjects[newObject.XPosition, newObject.YPosition]
                    .Add(newObject);

                _currentObjectCount++;
            }

            if (_currentObjectCount < count) return;
            _isWorldLoaded = true;
        }
    }

    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerSpawned;
        TreeExiting -= OnExiting;
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
            Array<WorldObject> cellContents =
                GetCellContents(coords.X, coords.Y);
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
        player.ActionController.GatherAction.GatherAttempted += OnPlayerGatherAction;
        player.ActionController.BuildAction.BlockPlaced += OnPlayerBuildAction;
        player.PlayerDespawned += OnPlayerDespawned;
        player.Inventory.PickedUpItem += OnPlayerPickedUpItem;
    }

    private void OnPlayerDespawned(Player player) {
        player.MovedCell -= OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted -= OnPlayerGatherAction;
        player.ActionController.BuildAction.BlockPlaced -= OnPlayerBuildAction;
        player.Inventory.PickedUpItem -= OnPlayerPickedUpItem;
        player.PlayerDespawned -= OnPlayerDespawned;
    }

    private void OnPlayerGatherAction(IntVector coords, Player player) {
        WorldObject worldObject = GetCellContents(coords.X, coords.Y)
            .FirstOrDefault(worldObject => worldObject is Block or Prop);

        float damage = player.PlayerEquipment.Pickaxe.Power;
        switch (worldObject) {
            case Block block: {
                block.CurrentHealth -= damage;
                if (block.CurrentHealth > 0) return;
                block.QueueFree();
                _activeWorldObjects[block.XPosition, block.YPosition]
                    .Remove(block);
                Vector2 position = new(
                    block.XPosition * Game.BlockSize,
                    block.YPosition * Game.BlockSize);
                CreatePickup(block.Item, position);
                break;
            }
            case Prop prop: {
                prop.CurrentHealth -= damage;
                if (prop.CurrentHealth > 0) return;
                prop.QueueFree();
                _activeWorldObjects[prop.XPosition, prop.YPosition]
                    .Remove(prop);
                Vector2 position = new(
                    prop.XPosition * Game.BlockSize,
                    prop.YPosition * Game.BlockSize);
                CreatePickup(prop.Item, position);
                break;
            }
            case null: {
                return;
            }
            default:
                return;
        }

        player.ActionController.GatherAction.OnAfterGatherSuccess();
    }

    private void OnPlayerBuildAction(Item item, IntVector coords) {
        Array<WorldObject> cellContents = GetCellContents(coords.X, coords.Y);
        if (cellContents.Count > 0) return;
        Block newBlock = Block.Create(new Dictionary {
            { "item", item.ToDictionary() },
            { "xPosition", coords.X },
            { "yPosition", coords.Y }
        });
        _activeWorldObjects[coords.X, coords.Y].Add(newBlock);
        _game.BlockParent.AddChild(newBlock, true);
        newBlock.Enable();
    }

    private void OnPlayerPickedUpItem(Pickup pickup) {
        DeletePickup(pickup);
    }

    private void CreatePickup(Item item, Vector2 position) {
        IntVector coords = new(position / Game.BlockSize);

        // new pickup needs data from item
        Pickup newPickup = Pickup.Create(new Dictionary {
            { "item", item.ToDictionary() },
            { "xPosition", coords.X },
            { "yPosition", coords.Y }
        });
        _activeWorldObjects[coords.X, coords.Y].Add(newPickup);
        newPickup.MovedCell += OnPickupMovedCell;

        _game.BlockParent.AddChild(newPickup, true);
    }


    // pickup manager


    private void DeletePickup(Pickup pickup) {
        int xPosition = pickup.XPosition;
        int yPosition = pickup.YPosition;
        _activeWorldObjects[xPosition, yPosition].Remove(pickup);
        pickup.QueueFree();
    }

    private void OnPickupMovedCell(Pickup pickup, Dictionary positionChange) {
        IntVector previousCoords = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector coords = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        _activeWorldObjects[previousCoords.X, previousCoords.Y].Remove(pickup);

        _activeWorldObjects[coords.X, coords.Y].Add(pickup);
    }
}