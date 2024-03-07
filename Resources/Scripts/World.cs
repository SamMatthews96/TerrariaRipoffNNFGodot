using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World : WorldBasicInfo, ISerializable {
    public SavedBlock[,] SavedBlocks { get; }

    public World(SavedBlock[,] savedBlocks, string name, int worldWidth, int worldHeight)
        : base(name, worldWidth, worldHeight) {
        SavedBlocks = savedBlocks;
    }

    public WorldBasicInfo GetBasicInfo() {
        return new WorldBasicInfo(Name, WorldWidth, WorldHeight);
    }
    
    public new Dictionary Serialize() {
        Dictionary serializedData = base.Serialize();
        Array savedBlocksSerialized = new();
        foreach (var block in SavedBlocks) {
            if (block is null) continue;
            savedBlocksSerialized.Add(block.Serialize());
        }
        serializedData.Add("SavedBlocks",savedBlocksSerialized);
        return serializedData;
    }
    
}