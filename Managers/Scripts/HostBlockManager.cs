using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostBlockManager : Node {
    [Export] private HostManager _hostManager;
    [Export] private PackedScene _savedBlockPackedScene;

    public const int BlockSize = 32;
    public const int BlockSpawnDistance = 20;

    private SavedBlock[,] _savedBlocks;

    [Signal] public delegate void SavedBlockDestroyedOnServerEventHandler(SavedBlock savedBlock);

    public void Initialize(Dictionary worldDictionary) {
        HostManager.RequireHost();

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


    // private void OnSavedBlockActiveBlockCreated(ActiveBlock activeBlock) {
    //     activeBlock.TakenDamage += OnActiveBlockTakenDamage;
    //     AddChild(activeBlock);
    // }

    // private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
    //     RpcId(MultiplayerManager.HostId, nameof(ServerDamageSavedBlock),
    //         activeBlock.SavedBlock.XPosition, activeBlock.SavedBlock.YPosition, damageAmount);
    // }

    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    // private void ServerDamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
    //     Rpc(nameof(DamageSavedBlock), xPosition, yPosition, damageAmount);
    // }

    // [Rpc(CallLocal = true)]
    // private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
    //     SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
    //     savedBlock?.TakeDamage(damageAmount);
    // }

    // private void OnServerSavedBlockHitZeroHealth(SavedBlock savedBlock) {
    //     EmitSignal(SignalName.SavedBlockDestroyedOnServer, savedBlock);
    //     Rpc(nameof(DestroySavedBlock), savedBlock.XPosition, savedBlock.YPosition);
    // }

    // [Rpc(CallLocal = true)]
    // private void DestroySavedBlock(int xPosition, int yPosition) {
    //     _savedBlocks[xPosition, yPosition].ActiveBlock.QueueFree();
    //     _savedBlocks[xPosition, yPosition] = null;
    // }


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