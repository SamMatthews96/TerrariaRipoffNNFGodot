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

    private bool _isStartAreaLoading = true;
    private bool _isWorldLoading;

    private Array _spawnFirst = new();
    private Array _spawnSecond = new();

    public event Action WorldLoaded;
    public event Action<Item, IntVector> CraftStationPlaced;

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
        if (_isStartAreaLoading) {
            ProcessLoadWorld(_spawnFirst, 16, out bool finished);
            if (finished) {
                _isStartAreaLoading = false;
                _isWorldLoading = true;
                WorldLoaded?.Invoke();
            }
        } else if (_isWorldLoading) {
            ProcessLoadWorld(_spawnSecond, 16, out bool finished);
            if (finished) {
                _isWorldLoading = false;
            }
        }
    }

    private void ProcessLoadWorld(Array spawnArray, float timeout, out bool finished) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (_currentObjectCount < spawnArray.Count &&
               stopwatch.ElapsedMilliseconds < timeout) {
            Dictionary savedWorldObjectDict =
                spawnArray[_currentObjectCount].AsGodotDictionary();

            WorldObject newObject = WorldObject.Create(savedWorldObjectDict);
            _game.BlockParent.AddChild(newObject, true);
            _activeWorldObjects[newObject.Coords.X, newObject.Coords.Y].Add(newObject);

            _currentObjectCount++;
        }

        if (_currentObjectCount == spawnArray.Count) {
            finished = true;
            _currentObjectCount = 0;
        } else {
            finished = false;
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
        player.ActionController.BuildAction.BuildActionAttempted += OnPlayerBuildAction;
        player.PlayerDespawned += OnPlayerDespawned;
        player.Inventory.PickedUpItem += OnPlayerPickedUpItem;
    }

    private void OnPlayerDespawned(Player player) {
        player.MovedCell -= OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted -= OnPlayerGatherAction;
        player.ActionController.BuildAction.BuildActionAttempted -= OnPlayerBuildAction;
        player.Inventory.PickedUpItem -= OnPlayerPickedUpItem;
        player.PlayerDespawned -= OnPlayerDespawned;
    }

    private void OnPlayerGatherAction(IntVector coords, Player player) {
        WorldObject worldObject = GetCellContents(coords.X, coords.Y)
            .FirstOrDefault(worldObject => worldObject is Block or Prop or PlaceableCell);
        if (worldObject is null) return;

        float damage = player.PlayerEquipment.Pickaxe.Power;
        Vector2 position = (worldObject.Coords * Game.BlockSize).ToVector2();
        switch (worldObject) {
            case Block block: {
                // @todo should be handled inside block
                block.CurrentHealth -= damage;
                if (block.CurrentHealth > 0) return;
                block.QueueFree();
                _activeWorldObjects[block.Coords.X, block.Coords.Y]
                    .Remove(block);
                CreatePickup(block.Item, position);
                break;
            }
            case Prop prop: {
                // @todo should be handled inside prop
                prop.CurrentHealth -= damage;
                if (prop.CurrentHealth > 0) return;
                prop.QueueFree();
                _activeWorldObjects[prop.Coords.X, prop.Coords.Y].Remove(prop);
                CreatePickup(prop.Item, position);
                break;
            }
            case PlaceableCell placeableCell: {
                placeableCell.OnGather();
                CreatePickup(placeableCell.Placeable.Item, position);
                break;
            }
            case null:
                return;
            default:
                return;
        }

        player.ActionController.GatherAction.OnAfterGatherSuccess();
    }

    private void OnPlayerBuildAction(Item item, IntVector coords) {
        Array<WorldObject> cellContents = GetCellContents(coords.X, coords.Y);
        if (item.HasProperty<ItemBlock>()) {
            if (cellContents.Any(worldObject => worldObject is Block or Prop)) {
                return;
            }

            Block newBlock = Block.Create(item, coords);
            _activeWorldObjects[coords.X, coords.Y].Add(newBlock);
            _game.BlockParent.AddChild(newBlock, true);
            newBlock.Enable();
        }

        if (item.HasProperty<ItemPlaceable>()) {
            ItemPlaceable itemPlaceable = item.GetProperty<ItemPlaceable>();
            List<IntVector> region = itemPlaceable.OccupiedCells
                .Select(cell => coords + cell).ToList();

            Array<WorldObject> worldObjects = GetObjectsInRegion(region);
            if (worldObjects.Any(worldObject => worldObject is Block or Prop)) {
                return;
            }

            Placeable placeable = Placeable.Create(item, coords);
            _activeWorldObjects[coords.X, coords.Y].Add(placeable);
            _game.BlockParent.AddChild(placeable, true);
            placeable.Enable();
            foreach (IntVector cell in region) {
                PlaceableCell placeableCell = PlaceableCell.Create(placeable, coords);
                _activeWorldObjects[cell.X, cell.Y].Add(placeableCell);
                _game.BlockParent.AddChild(placeableCell, true);
                placeableCell.Enable();
                placeable.RegisterCell(placeableCell);
            }

            placeable.Destroyed += OnPlaceableDestroyed;
        }

        if (item.HasProperty<ItemCraftStation>()) {
            CraftStationPlaced?.Invoke(item, coords);
        }
    }

    private void OnPlaceableDestroyed(Placeable placeable) {
        placeable.Destroyed -= OnPlaceableDestroyed;
        foreach (PlaceableCell placeableCell in placeable.PlaceableCell) {
            placeableCell.QueueFree();
        }

        placeable.QueueFree();
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
        _activeWorldObjects[pickup.Coords.X, pickup.Coords.Y].Remove(pickup);
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