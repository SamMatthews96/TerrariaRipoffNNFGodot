using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class ServerData : Node {
    [Export] private BlockType testBlockType;
    private SavedBlock[,] savedBlocks;
    [Export] public int WorldWidth { get; private set; }
    [Export] public int WorldHeight { get; private set; }
    [Export] public int SpawnX { get; private set; }
    [Export] public int SpawnY { get; private set; }

    public override void _Ready() {
        savedBlocks = new SavedBlock[WorldWidth, WorldHeight];
        for (int x = 0; x < WorldWidth; x++) {
            savedBlocks[x, 6] = new SavedBlock(testBlockType, x, 6);
        }
    }

    public SavedBlock GetSavedBlock(int x, int y) {
        return savedBlocks[x, y];
    }
}