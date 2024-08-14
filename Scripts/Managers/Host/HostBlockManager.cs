using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostBlockManager : Node {
    public static HostBlockManager Instance { get; private set; }
    
    [Export] private PackedScene _savedBlockPackedScene;

    public const int BlockSpawnDistance = 20;

    private SavedBlock[,] _savedBlocks;

    [Signal] public delegate void BlockDestroyedEventHandler(SavedBlock savedBlock);

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240814.0048.1] HostManager already instantiated");
        }

        Instance = this;
    }
    
    public void Initialize(Dictionary worldDictionary) {
        _savedBlocks = new SavedBlock[
            GameManager.Instance.Width, GameManager.Instance.Height];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();
        foreach (Dictionary savedBlockDict in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }
    }

    public void SpawnLocalBlocks(IntVector spawnPosition) {
        List<IntVector> region = GameManager.Instance.Region.GetRegion(spawnPosition, BlockSpawnDistance);
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(region);
        foreach (SavedBlock savedBlock in savedBlocks) {
            SpawnBlock(savedBlock);
        }
    }

    private void SpawnBlock(SavedBlock savedBlock) {
        ActiveBlock activeBlock = _savedBlockPackedScene.Instantiate<ActiveBlock>();
        activeBlock.Initialize(savedBlock);
        activeBlock.TakenDamage += OnActiveBlockTakenDamage;
        GameManager.Instance.BlockParent.AddChild(activeBlock, true);
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

    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        SavedBlock savedBlock = activeBlock.SavedBlock;
        savedBlock.CurrentHealth -= damageAmount;
        if (savedBlock.CurrentHealth > 0) return;
        _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = null;
        activeBlock.QueueFree();

        EmitSignal(SignalName.BlockDestroyed, savedBlock);
    }

    // private void OnLocalPlayerMoved(Player player) {
    //     IntVector oldCoordinates = new(player.PreviousXCoords, player.PreviousYCoords);
    //     IntVector newCoordinates = new(player.XCoords, player.YCoords);
    //     List<IntVector> newRegion = GetRegionDelta(
    //         newCoordinates, oldCoordinates, BlockRenderDistance);
    //
    //     List<SavedBlock> savedBlocksToWatch = GetSavedBlocksInRegion(newRegion);
    //     foreach (SavedBlock savedBlock in savedBlocksToWatch) {
    //         savedBlock.AddWatcher(player);
    //     }
    //
    //     List<IntVector> oldRegion = GetRegionDelta(
    //         oldCoordinates, newCoordinates, BlockRenderDistance);
    //     List<SavedBlock> savedBlocksToUnwatch = GetSavedBlocksInRegion(oldRegion);
    //     foreach (SavedBlock savedBlock in savedBlocksToUnwatch) {
    //         savedBlock.RemoveWatcher(player);
    //     }
    // }
}