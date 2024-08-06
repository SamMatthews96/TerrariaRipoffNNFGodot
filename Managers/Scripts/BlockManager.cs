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
        _savedBlocks = new SavedBlock[_width, _height];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();

        PlayerManager.Instance.LocalPlayerSpawned += OnPlayerManagerLocalPlayerSpawned;

        foreach (GodotDictionary savedBlockDictionary in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
            savedBlock.ActiveBlockCreated += OnSavedBlockActiveBlockCreated;
            if (MultiplayerManager.HOST_ID != Multiplayer.GetUniqueId()) continue;
            savedBlock.HitZeroHealth += OnServerSavedBlockHitZeroHealth;
        }
    }

    private void OnPlayerManagerLocalPlayerSpawned(int x, int y) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        List<IntVector> spawnRegion = GetRegion(new IntVector(x, y), BlockRenderDistance);
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(spawnRegion);
        foreach (SavedBlock savedBlock in savedBlocks) {
            savedBlock.AddWatcher(Player.LocalPlayer);
        }
    }

    private void OnSavedBlockActiveBlockCreated(ActiveBlock activeBlock) {
        activeBlock.TakenDamage += OnActiveBlockTakenDamage;
        AddChild(activeBlock);
    }
    
    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerDamageSavedBlock),
            activeBlock.SavedBlock.XPosition, activeBlock.SavedBlock.YPosition, damageAmount);
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

    private void OnServerSavedBlockHitZeroHealth(SavedBlock savedBlock) {
        Rpc(nameof(DestroySavedBlock), savedBlock.XPosition, savedBlock.YPosition);
    }

    [Rpc(CallLocal = true)]
    private void DestroySavedBlock(int xPosition, int yPosition) {
        _savedBlocks[xPosition, yPosition].ActiveBlock.QueueFree();
        _savedBlocks[xPosition, yPosition] = null;
    }
    
    
    private void OnLocalPlayerMoved(Player player) {
        IntVector oldCoordinates = new(player.PreviousXCoords, player.PreviousYCoords);
        IntVector newCoordinates = new(player.XCoords, player.YCoords);
        List<IntVector> newRegion = GetRegionDelta(
            newCoordinates, oldCoordinates, BlockRenderDistance);

        List<SavedBlock> savedBlocksToWatch = GetSavedBlocksInRegion(newRegion);
        foreach (SavedBlock savedBlock in savedBlocksToWatch) {
            savedBlock.AddWatcher(player);
        }

        List<IntVector> oldRegion = GetRegionDelta(
            oldCoordinates, newCoordinates, BlockRenderDistance);
        List<SavedBlock> savedBlocksToUnwatch = GetSavedBlocksInRegion(oldRegion);
        foreach (SavedBlock savedBlock in savedBlocksToUnwatch) {
            savedBlock.RemoveWatcher(player);
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
}