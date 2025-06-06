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
    private Array<ActiveWorldObject>[,] _activeWorldObjects;
    private Array _savedWorldObjects;
    private int _currentObjectCount;
    private bool _isWorldLoaded;

    public event Action WorldLoaded;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }

    public void SetGame(Game game, Dictionary worldData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;

        Player.PlayerSpawned += OnPlayerSpawned;

        _activeWorldObjects = new Array<ActiveWorldObject>[_game.Width, _game.Height];
        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
                _activeWorldObjects[x, y] = new Array<ActiveWorldObject>();
            }
        }

        _savedWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();

        TreeExiting += OnExiting;
    }

    public Array<ActiveWorldObject> GetCellContents(int x, int y) {
        return _activeWorldObjects[x, y];
    }

    public Array<T> GetCellContents<[MustBeVariant] T>(int x, int y)
        where T : ActiveWorldObject {
        Array<T> cellContentsOfType = new();
        foreach (ActiveWorldObject activeWorldObject in GetCellContents(x, y)) {
            if (activeWorldObject is T worldObject) {
                cellContentsOfType.Add(worldObject);
            }
        }

        return cellContentsOfType;
    }

    public override void _Process(double delta) {
        if (_isWorldLoaded) return;

        int count = _savedWorldObjects.Count;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (_currentObjectCount < count &&
               stopwatch.ElapsedMilliseconds < 16) {
            Dictionary savedWorldObjectDict =
                _savedWorldObjects[_currentObjectCount].AsGodotDictionary();
            if (savedWorldObjectDict["type"].AsString() == "block") {
                ActiveWorldObject newObject =
                    ActiveWorldObject.Create(savedWorldObjectDict);
                _game.BlockParent.AddChild(newObject, true);
                _activeWorldObjects[newObject.XPosition, newObject.YPosition]
                    .Add(newObject);
            }

            _currentObjectCount++;
        }

        if (_currentObjectCount < count) return;
        _isWorldLoaded = true;
        WorldLoaded?.Invoke();
    }

    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerSpawned;
        TreeExiting -= OnExiting;
    }

    private void EnableObjectsInRegion(List<IntVector> region) {
        Array<ActiveWorldObject> objects = GetObjectsInRegion(region);
        foreach (ActiveWorldObject worldObject in objects) {
            worldObject.Enable();
        }
    }

    private Array<ActiveWorldObject> GetObjectsInRegion(List<IntVector> region) {
        Array<ActiveWorldObject> objects = new();
        foreach (IntVector coords in region) {
            Array<ActiveWorldObject> cellContents =
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

    private void OnPlayerGatherAction(IntVector coords, float damage) {
        Array<ActiveBlock> cellContents =
            GetCellContents<ActiveBlock>(coords.X, coords.Y);
        if (cellContents.Count == 0) return;
        DamageActiveBlock(cellContents[0], damage);
    }

    private void OnPlayerBuildAction(Item item, IntVector coords) {
        Array<ActiveWorldObject> cellContents = GetCellContents(coords.X, coords.Y);
        if (cellContents.Count > 0) return;
        ActiveBlock newBlock = ActiveBlock.Create(new Dictionary {
            { "item", item.ToDictionary() },
            { "xPosition", coords.X },
            { "yPosition", coords.Y }
        });
        _activeWorldObjects[coords.X, coords.Y].Add(newBlock);
        _game.BlockParent.AddChild(newBlock, true);
        newBlock.Enable();
    }

    private void OnPlayerPickedUpItem(ActivePickup activePickup) {
        DeletePickup(activePickup);
    }

    private void DamageActiveBlock(ActiveBlock block, float damageAmount) {
        block.CurrentHealth -= damageAmount;
        if (block.CurrentHealth > 0) return;
        block.QueueFree();
        _activeWorldObjects[block.XPosition, block.YPosition]
            .Remove(block);

        OnBlockManagerBlockDestroyed(block);
    }

    private void OnBlockManagerBlockDestroyed(ActiveBlock block) {
        Vector2 position = new(
            block.XPosition * Game.BlockSize,
            block.YPosition * Game.BlockSize);

        CreatePickup(block.Item, position);
    }

    private void CreatePickup(Item item, Vector2 position) {
        IntVector coords = new(position / Game.BlockSize);

        // new pickup needs data from item
        ActivePickup newPickup = ActivePickup.Create(new Dictionary {
            { "item", item.ToDictionary() },
            { "xPosition", coords.X },
            { "yPosition", coords.Y }
        });
        _activeWorldObjects[coords.X, coords.Y].Add(newPickup);
        newPickup.MovedCell += OnPickupMovedCell;

        _game.BlockParent.AddChild(newPickup, true);
    }


    // pickup manager


    private void DeletePickup(ActivePickup activePickup) {
        int xPosition = activePickup.XPosition;
        int yPosition = activePickup.YPosition;
        _activeWorldObjects[xPosition, yPosition].Remove(activePickup);
        activePickup.QueueFree();
    }

    private void OnPickupMovedCell(ActivePickup activePickup, Dictionary positionChange) {
        IntVector previousCoords = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector coords = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        _activeWorldObjects[previousCoords.X, previousCoords.Y].Remove(activePickup);

        _activeWorldObjects[coords.X, coords.Y].Add(activePickup);
    }
}