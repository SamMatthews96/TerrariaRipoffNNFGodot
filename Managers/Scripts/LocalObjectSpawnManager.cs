using System;
using System.Collections.Generic;
using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;
using ActiveBlock = TerrariaRipoffNNF.GameObjects.Scripts.ActiveBlock;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class LocalObjectSpawnManager : Node {
    public const int BLOCK_RENDER_DISTANCE = 20;

    private int _width;
    private int _height;
    private ActiveBlock[,] activeBlocks;

    public void Initialize(int worldWidth, int worldHeight) {
        _width = worldWidth;
        _height = worldHeight;
        activeBlocks = new ActiveBlock[worldWidth, worldHeight];
    }

    private void OnPlayerManagerLocalPlayerSpawned(int x, int y) {
        List<IntVector> spawnRegion = GetRegion(new IntVector(x, y));
        List<SavedBlock> savedBlocks = WorldManager.Instance.GetSavedBlocksInRegion(spawnRegion);
        foreach (SavedBlock savedBlock in savedBlocks) {
            ActiveBlock activeBlock =
                ActiveBlock.Instantiate(savedBlock.BlockType, savedBlock.XPosition, savedBlock.YPosition);
            activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
            AddChild(activeBlock);
        }

        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
    }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        IntVector oldCoordinates = new IntVector(oldXCoordinate, oldYCoordinate);
        IntVector newCoordinates = new IntVector(newXCoordinate, newYCoordinate);
        List<IntVector> newRegion = GetRegionDelta(
            newCoordinates, oldCoordinates);
        
        List<SavedBlock> savedBlocks = WorldManager.Instance.GetSavedBlocksInRegion(newRegion);
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            ActiveBlock activeBlock =
                ActiveBlock.Instantiate(savedBlock.BlockType, savedBlock.XPosition, savedBlock.YPosition);
            activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
            AddChild(activeBlock);
        }

        List<IntVector> oldRegion = GetRegionDelta(
            oldCoordinates, newCoordinates);
        foreach (IntVector blockCoordinates in oldRegion) {
            ActiveBlock activeBlock = activeBlocks[blockCoordinates.X, blockCoordinates.Y];
            if (activeBlock is null) continue;
            activeBlock.QueueFree();
            activeBlocks[blockCoordinates.X, blockCoordinates.Y] = null;
        }
        
        
        
        // Remove blocks that are no longer in render distance
    }

    private List<IntVector> GetRegion(IntVector center) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, center.X - BLOCK_RENDER_DISTANCE);
        int xEnd = Math.Min(_width - 1, center.X + BLOCK_RENDER_DISTANCE);
        int yStart = Math.Max(0, center.Y - BLOCK_RENDER_DISTANCE);
        int yEnd = Math.Min(_height - 1, center.Y + BLOCK_RENDER_DISTANCE);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }

    private List<IntVector> GetRegionDelta(IntVector includeCenter, IntVector excludeCenter) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, includeCenter.X - BLOCK_RENDER_DISTANCE);
        int xEnd = Math.Min(_width - 1, includeCenter.X + BLOCK_RENDER_DISTANCE);
        int yStart = Math.Max(0, includeCenter.Y - BLOCK_RENDER_DISTANCE);
        int yEnd = Math.Min(_height - 1, includeCenter.Y + BLOCK_RENDER_DISTANCE);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                if (Math.Abs(x - excludeCenter.X) < BLOCK_RENDER_DISTANCE &&
                    Math.Abs(y - excludeCenter.Y) < BLOCK_RENDER_DISTANCE) continue;
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }
}