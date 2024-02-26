
using Godot;

[GlobalClass]
public partial class BlockType : Resource {
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture Texture { get; private set; }
    
}

