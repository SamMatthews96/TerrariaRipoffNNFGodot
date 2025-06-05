using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

    public event Action WorldLoaded;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
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

    private bool _isWorldLoaded;
    public override void _Process(double delta) {
        if (_isWorldLoaded) return;
        
        int count = _savedWorldObjects.Count;
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (_currentObjectCount < count ||
               stopwatch.ElapsedMilliseconds < 16) {
            Dictionary savedWorldObjectDict =
                _savedWorldObjects[_currentObjectCount].AsGodotDictionary();
            if (savedWorldObjectDict["type"].AsString() == "block") {
                ActiveWorldObject newObject =
                    ActiveWorldObject.Create(savedWorldObjectDict);
                _game.BlockParent.AddChild(newObject, true);
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

    private void OnPlayerSpawned(Player player) {
        // List<IntVector> region = _game.Region.GetRegion(
        //     player.SpawnCoords, BlockSpawnDistance);
        //
        // SpawnBlocksInRegion(region);
        // player.MovedCell += OnLocalPlayerMoved;
        // player.ActionController.GatherAction.GatherAttempted += OnPlayerGatherAction;
        // player.ActionController.BuildAction.BlockPlaced += OnPlayerBuildAction;
        // player.PlayerDespawned += OnPlayerDespawned;
        // player.Inventory.PickedUpItem += OnPlayerPickedUpItem;
    }

    // private void OnPlayerDespawned(Player player) {
    //     player.MovedCell -= OnLocalPlayerMoved;
    //     player.ActionController.GatherAction.GatherAttempted -= OnPlayerGatherAction;
    //     player.ActionController.BuildAction.BlockPlaced -= OnPlayerBuildAction;
    //     player.Inventory.PickedUpItem -= OnPlayerPickedUpItem;
    //     player.PlayerDespawned -= OnPlayerDespawned;
    // }

    // private void OnLocalPlayerMoved(Dictionary positionChange) {
    //     IntVector oldCoordinates = new(
    //         (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
    //     IntVector newCoordinates = new(
    //         (int)positionChange["X"], (int)positionChange["Y"]);
    //
    //     List<IntVector> newRegion = _game.Region.GetRegionDelta(
    //         newCoordinates, oldCoordinates, BlockSpawnDistance);
    //
    //     SpawnBlocksInRegion(newRegion);
    // }

    // private void OnPlayerGatherAction(IntVector coords, float damage) {
    //     Array<ActiveBlock> cellContents =
    //         GetCellContents<ActiveBlock>(coords.X, coords.Y);
    //     if (cellContents.Count == 0) return;
    //     DamageActiveBlock(cellContents[0], damage);
    // }

    // private void SpawnBlocksInRegion(List<IntVector> region) {
    //     List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(region);
    //     foreach (SavedBlock savedBlock in savedBlocks) {
    //         Array<ActiveBlock> cellContents =
    //             GetCellContents<ActiveBlock>(savedBlock.XPosition, savedBlock.YPosition);
    //         if (cellContents.Count == 0) {
    //             SpawnObject(savedBlock);
    //         }
    //     }
    // }

    // private void SpawnObject(SavedWorldObject savedWorldObject) {
    //     ActiveWorldObject activeBlock = savedWorldObject.SpawnActiveObject();
    //     _game.BlockParent.AddChild(activeBlock, true);
    //
    //     _activeWorldObjects[savedWorldObject.XPosition, savedWorldObject.YPosition]
    //         .Add(activeBlock);
    // }

    // private List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
    //     List<SavedBlock> savedBlocks = new();
    //     foreach (IntVector coords in region) {
    //         Array<SavedBlock> cellContents =
    //             GetSavedCellContents<SavedBlock>(coords.X, coords.Y);
    //         if (cellContents.Count == 0) continue;
    //         savedBlocks.Add(cellContents[0]);
    //     }
    //
    //     return savedBlocks;
    // }

    // private void DamageActiveBlock(ActiveBlock activeBlock, float damageAmount) {
    //     SavedBlock savedBlock = activeBlock.SavedBlock;
    //     savedBlock.CurrentHealth -= damageAmount;
    //     if (savedBlock.CurrentHealth > 0) return;
    //     _savedWorldObjects[savedBlock.XPosition, savedBlock.YPosition]
    //         .Remove(savedBlock);
    //     activeBlock.QueueFree();
    //     _activeWorldObjects[savedBlock.XPosition, savedBlock.YPosition]
    //         .Remove(activeBlock);
    //
    //     OnBlockManagerBlockDestroyed(savedBlock);
    // }

    // private void OnPlayerBuildAction(Item item, IntVector coords) {
    //     Array<SavedBlock> savedBlocks =
    //         GetSavedCellContents<SavedBlock>(coords.X, coords.Y);
    //     if (savedBlocks.Count > 0) return;
    //     SavedBlock savedBlock = SavedBlock.Create(
    //         block: item, xPosition: coords.X, yPosition: coords.Y
    //     );
    //     _savedWorldObjects[coords.X, coords.Y].Add(savedBlock);
    //     SpawnObject(savedBlock);
    // }

    // pickup manager

    // private void OnPlayerPickedUpItem(ActivePickup activePickup) {
    //     DeletePickup(activePickup);
    // }

    // private void DeletePickup(ActivePickup activePickup) {
    //     int xPosition = activePickup.SavedPickup.XPosition;
    //     int yPosition = activePickup.SavedPickup.YPosition;
    //     _activeWorldObjects[xPosition, yPosition].Remove(activePickup);
    //     _savedWorldObjects[xPosition, yPosition].Remove(activePickup.SavedPickup);
    //     activePickup.QueueFree();
    // }

    // private void OnBlockManagerBlockDestroyed(SavedBlock savedBlock) {
    //     Vector2 position = new(savedBlock.XPosition * Game.BlockSize,
    //         savedBlock.YPosition * Game.BlockSize);
    //
    //     CreatePickup(savedBlock.Item, position);
    // }

    // private void CreatePickup(Item item, Vector2 position) {
    //     IntVector coords = new(position / Game.BlockSize);
    //
    //
    //     ActivePickup activePickup = Data.PackedScenes.ActivePickup.Instantiate<ActivePickup>();
    //     activePickup.Initialize(savedPickup);
    //     _activeWorldObjects[coords.X, coords.Y].Add(activePickup);
    //     activePickup.MovedCell += OnPickupMovedCell;
    //
    //     _game.BlockParent.AddChild(activePickup, true);
    // }

    private void OnPickupMovedCell(ActivePickup activePickup, Dictionary positionChange) {
        IntVector previousCoords = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector coords = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        _activeWorldObjects[previousCoords.X, previousCoords.Y].Remove(activePickup);

        _activeWorldObjects[coords.X, coords.Y].Add(activePickup);
    }
}