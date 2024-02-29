using Godot;
using System;

public partial class ServerData : Node {
    private SavedBlock[,] savedBlocks;
    [Export] public int WorldWidth { get; private set; }
    [Export] public int WorldHeight { get; private set; }
    [Export] public int SpawnX { get; private set; }
    [Export] public int SpawnY { get; private set; }

    [Export] private BlockType testBlockType;

    public override void _Ready() {
        savedBlocks = new SavedBlock[WorldWidth, WorldHeight];
        for (int x = 0; x < WorldWidth; x++) {
            savedBlocks[x, 6] = new SavedBlock(testBlockType, x, 6);
        }
        savedBlocks[0, 0] = new SavedBlock(testBlockType, 0, 0);
        savedBlocks[1, 1] = new SavedBlock(testBlockType, 1, 1);
        savedBlocks[1, 0] = new SavedBlock(testBlockType, 1, 0);

    }

    public SavedBlock GetSavedBlock(int x, int y) {
        return savedBlocks[x, y];
    }
}