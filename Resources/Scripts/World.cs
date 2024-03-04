using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : WorldBasicInfo {
    public SavedBlock[,] SavedBlocks { get; private set; }

    public World(SavedBlock[,] savedBlocks, string name, int worldWidth, int worldHeight)
        : base(name, worldWidth, worldHeight) {
        SavedBlocks = savedBlocks;
    }
}