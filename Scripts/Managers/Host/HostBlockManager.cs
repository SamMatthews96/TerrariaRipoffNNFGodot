using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostBlockManager : Node {

    [Export] private PackedScene _blockPackedScene;

    public const int BlockSpawnDistance = 20;

    private SavedBlock[,] _savedBlocks;
    private ActiveBlock[,] _activeBlocks;

    public event Action<SavedBlock> BlockDestroyed;

    public void Initialize(Dictionary worldDictionary) {
        _savedBlocks = new SavedBlock[
            Manager.Instance.Game.Width, Manager.Instance.Game.Height];
        _activeBlocks = new ActiveBlock[
            Manager.Instance.Game.Width, Manager.Instance.Game.Height];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();
        foreach (Dictionary savedBlockDict in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }

        Host.Instance.HostPlayerManager.PlayerSpawned += OnPlayerManagerPlayerSpawned;
    }

    public void SpawnBlocksInRegion(List<IntVector> region) {
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

        ActiveBlock activeBlock = ActiveBlock.New(
            Manager.Instance.Game.BlockParent, _blockPackedScene, savedBlock).Build();
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

    private void OnPlayerManagerPlayerSpawned(Player player) {
        player.MovedCell += OnLocalPlayerMoved;
        player.GatherAttempted += OnPlayerGatherAction;
    }

    private void OnLocalPlayerMoved(Dictionary positionChange) {
        IntVector oldCoordinates = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector newCoordinates = new(
            (int)positionChange["X"], (int)positionChange["Y"]);

        List<IntVector> newRegion = Manager.Instance.Game.Region.GetRegionDelta(
            newCoordinates, oldCoordinates, BlockSpawnDistance);

        SpawnBlocksInRegion(newRegion);
    }

    private void OnPlayerGatherAction(IntVector coords, float damage) {
        ActiveBlock activeBlock = _activeBlocks[coords.X, coords.Y];
        if (activeBlock is null) return;
        DamageActiveBlock(activeBlock, damage);
    }
}