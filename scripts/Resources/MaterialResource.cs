using Godot;

namespace TerrariaRipoffNNF.scripts.Resources; 

[GlobalClass]
public partial class MaterialResource : ItemResource {
    [Export] public string[] Types { get; set; }
    [Export] public int Rank { get; set; }
}