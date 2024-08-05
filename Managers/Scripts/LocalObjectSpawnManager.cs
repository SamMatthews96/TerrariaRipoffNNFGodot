using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;
using ActiveBlock = TerrariaRipoffNNF.GameObjects.Scripts.ActiveBlock;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class LocalObjectSpawnManager : Node{
    private const int BLOCK_RENDER_DISTANCE = 20;

    private int _width;
    private int _height;
    private ActiveBlock[,] _activeBlocks;

    public static LocalObjectSpawnManager Instance { get; private set; }

    [Signal]
    public delegate void ActiveBlockTakenDamageEventHandler(ActiveBlock activeBlock, float damageAmount);

    public override void _EnterTree() {
        Instance = this;
    }

    public void Initialize(int worldWidth, int worldHeight) {
        _width = worldWidth;
        _height = worldHeight;
        _activeBlocks = new ActiveBlock[worldWidth, worldHeight];
        BlockManager.Instance.SavedBlockDestroyed += OnBlockManagerSavedBlockDestroyed;
        ItemPickupManager.Instance.SavedItemPickupCreated += OnItemPickupManagerSavedItemPickupCreated;
        PlayerManager.Instance.LocalPlayerSpawned += OnPlayerManagerLocalPlayerSpawned;
    }

    private void OnPlayerManagerLocalPlayerSpawned(int x, int y) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        List<IntVector> spawnRegion = GetRegion(new IntVector(x, y), BLOCK_RENDER_DISTANCE);
        List<SavedBlock> savedBlocks = BlockManager.Instance.GetSavedBlocksInRegion(spawnRegion);
        foreach (SavedBlock savedBlock in savedBlocks) {
            savedBlock.AddWatcher(this);
        }
    }

    private void OnLocalPlayerMoved(Player player) {
        IntVector oldCoordinates = new(player.PreviousXCoords, player.PreviousYCoords);
        IntVector newCoordinates = new(player.XCoords, player.YCoords);
        List<IntVector> newRegion = GetRegionDelta(
            newCoordinates, oldCoordinates, BLOCK_RENDER_DISTANCE);

        List<SavedBlock> savedBlocksToWatch = BlockManager.Instance.GetSavedBlocksInRegion(newRegion);
        foreach (SavedBlock savedBlock in savedBlocksToWatch) {
            if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            savedBlock.AddWatcher(player);
        }

        List<IntVector> oldRegion = GetRegionDelta(
            oldCoordinates, newCoordinates, BLOCK_RENDER_DISTANCE);
        List<SavedBlock> savedBlocksToUnwatch = BlockManager.Instance.GetSavedBlocksInRegion(oldRegion);
        foreach (SavedBlock savedBlock in savedBlocksToUnwatch) {
            if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            savedBlock.RemoveWatcher(player);
        }
    }

    private void OnBlockManagerSavedBlockDestroyed(SavedBlock savedBlock) {
        DeleteActiveBlock(savedBlock.XPosition, savedBlock.YPosition);
    }

    private void OnItemPickupManagerSavedItemPickupCreated(SavedItemPickup savedItemPickup) {
        if (!IsInRenderDistance(savedItemPickup)) return;
        ActiveItemPickup activeItemPickup = ActiveItemPickup.Initialize(savedItemPickup);
        AddChild(activeItemPickup);
    }

    public void CreateActiveBlock(SavedBlock savedBlock) {
        ActiveBlock activeBlock = ActiveBlock.Instantiate(savedBlock);
        _activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
        activeBlock.TakenDamage += OnActiveBlockTakenDamage;

        AddChild(activeBlock);
    }

    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        EmitSignal(SignalName.ActiveBlockTakenDamage, activeBlock, damageAmount);
    }

    private void DeleteActiveBlock(int x, int y) {
        ActiveBlock activeBlock = _activeBlocks[x, y];
        if (activeBlock is null) return;
        activeBlock.QueueFree();
        _activeBlocks[x, y] = null;
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
                if (Math.Abs(x - excludeCenter.X) < BLOCK_RENDER_DISTANCE &&
                    Math.Abs(y - excludeCenter.Y) < BLOCK_RENDER_DISTANCE) continue;
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }

    private bool IsInRenderDistance(ISavedGameObject savedGameObject) {
        IntVector delta = Player.LocalPlayer.GridPosition - savedGameObject.GridPosition;
        return delta.X < BLOCK_RENDER_DISTANCE && delta.Y < BLOCK_RENDER_DISTANCE;
    }
}