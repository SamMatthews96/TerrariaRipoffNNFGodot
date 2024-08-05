using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.GameObjects.Scripts;
using GodotDictionary = Godot.Collections.Dictionary;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class BlockManager : Node {
    public const int BLOCK_SIZE = 32;

    private int _width;
    private int _height;
    private SavedBlock[,] _savedBlocks;

    [Signal]
    public delegate void SavedBlockDestroyedEventHandler(SavedBlock savedBlock);

    [Signal]
    public delegate void SavedBlockDestroyedOnServerEventHandler(SavedBlock savedBlock);

    public delegate void SavedBlockWatchersBecomeNonZeroEventHandler(SavedBlock savedBlock, Node watcher);

    public delegate void SavedBlockWatchersBecomeZeroEventHandler(SavedBlock savedBlock, Node watcher);

    public static BlockManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public Array SavedBlocksToArray() {
        return SavedBlock.SerializeArray(_savedBlocks);
    }

    public void Initialize(GodotDictionary worldDictionary) {
        LocalObjectSpawnManager.Instance.ActiveBlockTakenDamage += OnActiveBlockTakenDamage;

        _width = (int)worldDictionary["Width"];
        _height = (int)worldDictionary["Height"];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();
        _savedBlocks = new SavedBlock[_width, _height];
        foreach (GodotDictionary savedBlockDictionary in savedBlockArray) {
            
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            if (MultiplayerManager.HOST_ID != Multiplayer.GetUniqueId()) continue;

            savedBlock.WatchersBecomeNonZero += OnSavedBlockWatchersBecomeNonZero;
            savedBlock.WatchersBecomeZero += OnSavedBlockWatchersBecomeZero;
            savedBlock.HitZeroHealth += OnServerSavedBlockHitZeroHealth;
        }
    }

    public List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = new();
        foreach (IntVector coords in region) {
            SavedBlock savedBlock = _savedBlocks[coords.X, coords.Y];
            if (savedBlock is null) continue;
            savedBlocks.Add(savedBlock);
        }

        return savedBlocks;
    }

    #region Block Changes

    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerDamageSavedBlock),
            activeBlock.SavedBlock.XPosition, activeBlock.SavedBlock.YPosition, damageAmount);
    }

    private void OnSavedBlockWatchersBecomeNonZero(SavedBlock savedBlock) {
        LocalObjectSpawnManager.Instance.CreateActiveBlock(savedBlock);
    }

    private void OnSavedBlockWatchersBecomeZero(SavedBlock savedBlock) {
        DestroySavedBlock(savedBlock.XPosition, savedBlock.YPosition);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerDamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        Rpc(nameof(DamageSavedBlock), xPosition, yPosition, damageAmount);
    }

    [Rpc(CallLocal = true)]
    private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
        savedBlock?.TakeDamage(damageAmount);
    }

    private void OnServerSavedBlockHitZeroHealth(int xPosition, int yPosition) {
        SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
        EmitSignal(SignalName.SavedBlockDestroyedOnServer, savedBlock);
        Rpc(nameof(DestroySavedBlock), xPosition, yPosition);
    }

    [Rpc(CallLocal = true)]
    private void DestroySavedBlock(int xPosition, int yPosition) {
        SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
        if (savedBlock is null) return;
        _savedBlocks[xPosition, yPosition] = null;
        EmitSignal(SignalName.SavedBlockDestroyed, savedBlock);
    }

    // private void OnPlayerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
    //     RpcId(MultiplayerManager.HOST_ID, nameof(ServerAttemptBuildBlock),
    //         xPosition, yPosition, blockResourcePath);
    // }
    //
    //
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    // private void ServerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
    //     BlockType blockType = BlockType.Deserialize(blockResourcePath);
    //     if (!_world.AreCoordsInBounds(xPosition, yPosition)) return;
    //     if (_world.SavedBlocks[xPosition, yPosition] is not null) return;
    //     SavedBlock savedBlock = SavedBlock.Builder.New(blockType, xPosition, yPosition).Build();
    //     _world.SavedBlocks[xPosition, yPosition] = savedBlock;
    // }

    #endregion block changes
}