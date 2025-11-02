using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class IngredientToOutputStringMap : Resource {
    [Export] public Dictionary<ItemIngredient, string> Map;
}