using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class CraftingStation : Resource {
    [Export] public CraftingStationType Type;
    [Export] public Texture2D Icon;
}