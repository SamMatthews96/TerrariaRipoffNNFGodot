
using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts; 

[GlobalClass]
public partial class BlockType : Resource {
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    
}