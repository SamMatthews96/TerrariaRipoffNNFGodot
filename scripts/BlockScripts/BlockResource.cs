using Godot;

namespace TerrariaRipoffNNF.scripts; 

public partial class BlockResource : Resource {
    [Export] public string Name;
    [Export] public float Weight;
    [Export] public float TensileStrength;
    [Export] public float MaxHealth;
}