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
            savedBlocks[x, 0] = new SavedBlock(testBlockType, x, 0);
        }
    }
}