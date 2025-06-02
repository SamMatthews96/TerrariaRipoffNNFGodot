using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node {
    public const int BlockSpawnDistance = 20;

    private Game _game;

    private Array<SavedWorldObject>[,] _savedWorldObjects;
    private Array<ActiveWorldObject>[,] _activeWorldObjects;

    public event Action WorldLoaded;
    public event Action<SavedBlock> BlockDestroyed;

    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }
    
    public Array<SavedWorldObject> GetSavedCellContents(int x, int y) {
        return _savedWorldObjects[x, y] ?? new Array<SavedWorldObject>();
    }

    public Array<T> GetSavedCellContents<[MustBeVariant] T>(int x, int y)
        where T : SavedWorldObject {
        Array<T> cellContentsOfType = new();
        foreach (SavedWorldObject savedWorldObject in GetSavedCellContents(x, y)) {
            if (savedWorldObject is T worldObject) {
                cellContentsOfType.Add(worldObject);
            }
        }

        return cellContentsOfType;
    }

    public Array<ActiveWorldObject> GetActiveCellContents(int x, int y) {
        return _activeWorldObjects[x, y] ?? new Array<ActiveWorldObject>();
    }

    public Array<T> GetActiveCellContents<[MustBeVariant] T>(int x, int y)
        where T : ActiveWorldObject {
        Array<T> cellContentsOfType = new();
        foreach (ActiveWorldObject activeWorldObject in GetActiveCellContents(x, y)) {
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
        Task task = Task.Run(() => HostCreateWorld(worldData));
        task.GetAwaiter().OnCompleted(() => { WorldLoaded?.Invoke(); });
        TreeExiting += OnExiting;
    }
    
    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerSpawned;
        TreeExiting -= OnExiting;
    }

    private void HostCreateWorld(Dictionary worldData) {
        _savedWorldObjects = new Array<SavedWorldObject>[_game.Width, _game.Height];
        _activeWorldObjects = new Array<ActiveWorldObject>[_game.Width, _game.Height];
        
        Array savedBlockArray = worldData["SavedBlocks"].AsGodotArray();
        foreach (Dictionary savedBlockDict in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
            _savedWorldObjects[savedBlock.XPosition, savedBlock.YPosition] ??=
                new Array<SavedWorldObject>();
            _savedWorldObjects[savedBlock.XPosition, savedBlock.YPosition]
                .Add(savedBlock);
        }
    }
    private void OnPlayerSpawned(Player player) {
        List<IntVector> region = _game.Region.GetRegion(
            player.SpawnCoords, BlockSpawnDistance);

        SpawnBlocksInRegion(region);
        player.MovedCell += OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted += OnPlayerGatherAction;
        player.ActionController.BuildAction.BlockPlaced += OnPlayerBuildAction;
        player.PlayerDespawned += OnPlayerDespawned;
    }

    private void OnPlayerDespawned(Player player) {
        player.MovedCell -= OnLocalPlayerMoved;
        player.ActionController.GatherAction.GatherAttempted -= OnPlayerGatherAction;
        player.ActionController.BuildAction.BlockPlaced -= OnPlayerBuildAction;
        player.PlayerDespawned -= OnPlayerDespawned;
    }

    private void OnLocalPlayerMoved(Dictionary positionChange) {
        IntVector oldCoordinates = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector newCoordinates = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        List<IntVector> newRegion = _game.Region.GetRegionDelta(
            newCoordinates, oldCoordinates, BlockSpawnDistance);

        SpawnBlocksInRegion(newRegion);
    }

    private void OnPlayerGatherAction(IntVector coords, float damage) {
        Array<ActiveBlock> cellContents =
            GetActiveCellContents<ActiveBlock>(coords.X, coords.Y);
        if (cellContents.Count == 0) return;
        DamageActiveBlock(cellContents[0], damage);
    }

    private void SpawnBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(region);
        foreach (SavedBlock savedBlock in savedBlocks) {
            Array<SavedBlock> cellContents =
                GetSavedCellContents<SavedBlock>(savedBlock.XPosition, savedBlock.YPosition);
            if (cellContents.Count == 0) {
                SpawnBlock(savedBlock);
            }
        }
    }

    private void SpawnBlock(SavedBlock savedBlock) {
        Array<ActiveBlock> cellContents =
            GetActiveCellContents<ActiveBlock>(savedBlock.XPosition, savedBlock.YPosition);
        if (cellContents.Count > 0) {
            throw new Exception("[20240814.2208.1] Block already spawned");
        }

        ActiveBlock activeBlock = ActiveBlock.Create(savedBlock);

        _game.BlockParent.AddChild(activeBlock, true);

        Array<ActiveWorldObject> activeCellObjects =
            _activeWorldObjects[savedBlock.XPosition, savedBlock.YPosition] ??=
                new Array<ActiveWorldObject>();
        activeCellObjects.Add(activeBlock);
    }

    private List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = new();
        foreach (IntVector coords in region) {
            Array<SavedBlock> cellContents =
                GetSavedCellContents<SavedBlock>(coords.X, coords.Y);
            if (cellContents.Count == 0) continue;
            savedBlocks.Add(cellContents[0]);
        }

        return savedBlocks;
    }
    
    private void DamageActiveBlock(ActiveBlock activeBlock, float damageAmount) {
        SavedBlock savedBlock = activeBlock.SavedBlock;
        savedBlock.CurrentHealth -= damageAmount;
        if (savedBlock.CurrentHealth > 0) return;
        _savedWorldObjects[savedBlock.XPosition, savedBlock.YPosition]
            .Remove(savedBlock);
        activeBlock.QueueFree();
        _activeWorldObjects[savedBlock.XPosition, savedBlock.YPosition]
            .Remove(activeBlock);
        BlockDestroyed?.Invoke(savedBlock);
    }

    private void OnPlayerBuildAction(Item item, IntVector coords) {
        Array<SavedBlock> savedBlocks =
            GetSavedCellContents<SavedBlock>(coords.X, coords.Y);
        if (savedBlocks.Count > 0) return;
        SavedBlock savedBlock = SavedBlock.Create(
            block: item, xPosition: coords.X, yPosition: coords.Y
        );
        Array<SavedWorldObject> savedWorldObjects =
            _savedWorldObjects[coords.X, coords.Y]
                ??= new Array<SavedWorldObject>();
        savedWorldObjects.Add(savedBlock);
        SpawnBlock(savedBlock);
    }
}