using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts;

[GlobalClass]
public partial class BlockType : InventoryItemType {
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture2D Texture { get; private set; }

    public string Serialize() {
        return ResourcePath;
    }

    public static BlockType FromString(string resourcePath) {
        return ResourceLoader.Load<BlockType>(resourcePath);
    }
}