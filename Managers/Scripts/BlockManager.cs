using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.GameObjects.Scripts;
using GodotDictionary = Godot.Collections.Dictionary;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class BlockManager : Node {
    public const int BlockSize = 32;
    private const int BlockRenderDistance = 20;

    private int _width;
    private int _height;
    private SavedBlock[,] _savedBlocks;
    // private ActiveBlock[,] _activeBlocks;

    [Signal]
    public delegate void SavedBlockDestroyedEventHandler(SavedBlock savedBlock);

    [Signal]
    public delegate void SavedBlockDestroyedOnServerEventHandler(SavedBlock savedBlock);

    public static BlockManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public Array SavedBlocksToArray() {
        return SavedBlock.SerializeArray(_savedBlocks);
    }

    public void Initialize(GodotDictionary worldDictionary) {
        _width = (int)worldDictionary["Width"];
        _height = (int)worldDictionary["Height"];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();
        _savedBlocks = new SavedBlock[_width, _height];
        // _activeBlocks = new ActiveBlock[_width, _height];

        PlayerManager.Instance.LocalPlayerSpawned += OnPlayerManagerLocalPlayerSpawned;

        foreach (GodotDictionary savedBlockDictionary in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            if (MultiplayerManager.HOST_ID != Multiplayer.GetUniqueId()) continue;

            // savedBlock.WatchersBecomeNonZero += OnSavedBlockWatchersBecomeNonZero;
            // savedBlock.WatchersBecomeZero += OnSavedBlockWatchersBecomeZero;
            savedBlock.HitZeroHealth += OnServerSavedBlockHitZeroHealth;
        }
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

    #region Block Changes

    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerDamageSavedBlock),
            activeBlock.SavedBlock.XPosition, activeBlock.SavedBlock.YPosition, damageAmount);
    }

    // private void OnSavedBlockWatchersBecomeNonZero(SavedBlock savedBlock) {
    //     CreateActiveBlock(savedBlock);
    // }
    //
    // private void OnSavedBlockWatchersBecomeZero(SavedBlock savedBlock) {
    //     DestroySavedBlock(savedBlock.XPosition, savedBlock.YPosition);
    // }

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
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    // private void ServerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
    //     BlockType blockType = BlockType.Deserialize(blockResourcePath);
    //     if (!_world.AreCoordsInBounds(xPosition, yPosition)) return;
    //     if (_world.SavedBlocks[xPosition, yPosition] is not null) return;
    //     SavedBlock savedBlock = SavedBlock.Builder.New(blockType, xPosition, yPosition).Build();
    //     _world.SavedBlocks[xPosition, yPosition] = savedBlock;
    // }

    #endregion block changes

    #region active blocks

    private void CreateActiveBlock(SavedBlock savedBlock) {
        ActiveBlock activeBlock = ActiveBlock.Instantiate(savedBlock);
        savedBlock.AddActiveBlock(activeBlock);
        activeBlock.TakenDamage += OnActiveBlockTakenDamage;

        AddChild(activeBlock);
    }

    private void DeleteActiveBlock(SavedBlock savedBlock) {
        savedBlock.ActiveBlock.QueueFree();
        savedBlock.RemoveActiveBlock();
    }

    private void OnLocalPlayerMoved(Player player) {
        IntVector oldCoordinates = new(player.PreviousXCoords, player.PreviousYCoords);
        IntVector newCoordinates = new(player.XCoords, player.YCoords);
        List<IntVector> newRegion = GetRegionDelta(
            newCoordinates, oldCoordinates, BlockRenderDistance);

        List<SavedBlock> savedBlocksToWatch = GetSavedBlocksInRegion(newRegion);
        foreach (SavedBlock savedBlock in savedBlocksToWatch) {
            AddWatcher(savedBlock, player);
        }

        List<IntVector> oldRegion = GetRegionDelta(
            oldCoordinates, newCoordinates, BlockRenderDistance);
        List<SavedBlock> savedBlocksToUnwatch = GetSavedBlocksInRegion(oldRegion);
        foreach (SavedBlock savedBlock in savedBlocksToUnwatch) {
            DeleteWatcher(savedBlock, player);
        }
    }

    private void AddWatcher(SavedBlock savedBlock, Node watcher) {
        savedBlock.AddWatcher(watcher);
        if (!savedBlock.ShouldCreateActiveBlock) return;
        CreateActiveBlock(savedBlock);
    }
    
    private void DeleteWatcher(SavedBlock savedBlock, Node watcher) {
        savedBlock.RemoveWatcher(watcher);
        if (!savedBlock.ShouldDeleteActiveBlock) return;
        DeleteActiveBlock(savedBlock);
    }

    private void OnPlayerManagerLocalPlayerSpawned(int x, int y) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        List<IntVector> spawnRegion = GetRegion(new IntVector(x, y), BlockRenderDistance);
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(spawnRegion);
        foreach (SavedBlock savedBlock in savedBlocks) {
            AddWatcher(savedBlock, Player.LocalPlayer);
        }
    }

    private List<IntVector> GetRegion(IntVector center, int distanceToEdge) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, center.X - distanceToEdge);
        int xEnd = Math.Min(_width - 1, center.X + distanceToEdge);
        int yStart = Math.Max(0, center.Y - distanceToEdge);
        int yEnd = Math.Min(_height - 1, center.Y + distanceToEdge);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }

    private List<IntVector> GetRegionDelta(IntVector includeCenter, IntVector excludeCenter, int distanceToEdge) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, includeCenter.X - distanceToEdge);
        int xEnd = Math.Min(_width - 1, includeCenter.X + distanceToEdge);
        int yStart = Math.Max(0, includeCenter.Y - distanceToEdge);
        int yEnd = Math.Min(_height - 1, includeCenter.Y + distanceToEdge);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                if (Math.Abs(x - excludeCenter.X) < BlockRenderDistance &&
                    Math.Abs(y - excludeCenter.Y) < BlockRenderDistance) continue;
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }

    #endregion
}