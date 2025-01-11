using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PlaceableCrafting : PlaceableProperty {
    [Export] public CraftingStation CraftingStation { get; private set; }
}