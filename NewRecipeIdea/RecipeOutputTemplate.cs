using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeOutputTemplate : Resource {
    [Export] public RecipePropertyMapMultiplier InventorySpace { get; private set; }
}