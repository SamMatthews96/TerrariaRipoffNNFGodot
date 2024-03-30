using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using ActiveBlock = TerrariaRipoffNNF.GameObjects.Scripts.ActiveBlock;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class LocalObjectSpawnManager : Node {
    public const int BLOCK_RENDER_DISTANCE = 20;

    private ActiveBlock[,] activeBlocks;

    public void Initialize(int worldWidth, int worldHeight) {
        activeBlocks = new ActiveBlock[worldWidth, worldHeight];
    }

    private void OnPlayerManagerLocalPlayerSpawned(int x, int y) {
        List<SavedBlock> savedBlocks = WorldManager.Instance.GetSavedBlocksInRegion(x, y);
        foreach (SavedBlock savedBlock in savedBlocks) {
            ActiveBlock activeBlock =
                ActiveBlock.Instantiate(savedBlock.BlockType, savedBlock.XPosition, savedBlock.YPosition);
            activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
            AddChild(activeBlock);
        }
    }

    private void OnLocalPlayerMoved(
        int newXCoordinate, int newYCoordinate, int oldXCoordinate, int oldYCoordinate) {
        List<SavedBlock> savedBlocks = WorldManager.Instance.GetSavedBlocksInRegion(newXCoordinate, newYCoordinate);
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            ActiveBlock activeBlock =
                ActiveBlock.Instantiate(savedBlock.BlockType, savedBlock.XPosition, savedBlock.YPosition);
            activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
            AddChild(activeBlock);
        }

        // Remove blocks that are no longer in render distance
    }
}