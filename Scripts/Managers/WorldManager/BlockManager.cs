using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BlockManager : Node {
    public const int BlockSpawnDistance = 20;

    private Game _game;

    private SavedBlock[,] _savedBlocks;
    private ActiveBlock[,] _activeBlocks;
    
    public event Action WorldLoaded;
    public event Action<SavedBlock> BlockDestroyed;

    public void SetGame(Game game, Dictionary worldData) {
        if (_game is not null) throw new Exception("[20240814.2208.1] Game already set");
        _game = game;
        Player.PlayerSpawned += OnPlayerManagerPlayerSpawned;
        Task task = Task.Run(() => HostCreateWorld(worldData));
        task.GetAwaiter().OnCompleted(() => { WorldLoaded?.Invoke(); });
        TreeExiting += OnExiting;
    }

    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerManagerPlayerSpawned;
        TreeExiting -= OnExiting;
    }

    private void HostCreateWorld(Dictionary worldData) {
        _savedBlocks = new SavedBlock[_game.Width, _game.Height];
        _activeBlocks = new ActiveBlock[_game.Width, _game.Height];

        Array savedBlockArray = worldData["SavedBlocks"].AsGodotArray();
        foreach (Dictionary savedBlockDict in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
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

    private void SpawnBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(region);
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            SpawnBlock(savedBlock);
        }
    }

    private void SpawnBlock(SavedBlock savedBlock) {
        if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) {
            throw new Exception("[20240814.2208.1] Block already spawned");
        }

        ActiveBlock activeBlock = ActiveBlock.Create(savedBlock);

        _game.BlockParent.AddChild(activeBlock, true);
        _activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
    }

    private void DamageActiveBlock(ActiveBlock activeBlock, float damageAmount) {
        SavedBlock savedBlock = activeBlock.SavedBlock;
        savedBlock.CurrentHealth -= damageAmount;
        if (savedBlock.CurrentHealth > 0) return;
        _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = null;
        activeBlock.QueueFree();
        _activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = null;

        BlockDestroyed?.Invoke(savedBlock);
    }

    private List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = new();
        foreach (IntVector coords in region) {
            SavedBlock savedBlock = _savedBlocks[coords.X, coords.Y];
            if (savedBlock is null) continue;
            savedBlocks.Add(savedBlock);
        }

        return savedBlocks;
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
        ActiveBlock activeBlock = _activeBlocks[coords.X, coords.Y];
        if (activeBlock is null) return;
        DamageActiveBlock(activeBlock, damage);
    }

    private void OnPlayerBuildAction(Item item, IntVector coords) {
        if (_savedBlocks[coords.X, coords.Y] is not null) return;
        SavedBlock savedBlock = SavedBlock.Create(
            block: item, xPosition: coords.X, yPosition: coords.Y
        );
        _savedBlocks[coords.X, coords.Y] = savedBlock;
        SpawnBlock(savedBlock);
    }
}