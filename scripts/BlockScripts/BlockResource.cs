
using Godot;

namespace TerrariaRipoffNNF.scripts.BlockScripts; 

[GlobalClass]
public partial class BlockResource : Resource {
    [Export] public string Name { get; set; }
    [Export] public float Weight { get; set; }
    [Export] public float TensileStrength { get; set; }
    [Export] public float MaxHealth { get; set; }
    
}

