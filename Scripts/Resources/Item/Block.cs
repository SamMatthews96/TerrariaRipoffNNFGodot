using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Block : ItemProperty {
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture2D Texture { get; private set; }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath", ResourcePath);
        return serialized;
    }
}