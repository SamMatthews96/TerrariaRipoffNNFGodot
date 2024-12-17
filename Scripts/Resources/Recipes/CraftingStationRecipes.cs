using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;
[GlobalClass]
public partial class CraftingStationRecipes : Resource{
    [Export] public CraftingStationType CraftingStationType { get; private set; }
    [Export] public Array<Recipe> Recipes { get; private set; }
}