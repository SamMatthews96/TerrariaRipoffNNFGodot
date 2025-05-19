using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeOutputTemplate : Resource {
    [Export] public RecipePropertyMapMultiplier InventorySpace { get; private set; }
    [Export] public RecipePropertyMapString Name { get; private set; }
}