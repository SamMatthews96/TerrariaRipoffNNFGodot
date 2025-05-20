using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class IngredientNameToOutputNameMap : Resource {
    [Export] public Dictionary<IngredientProperty, string> Map;
}