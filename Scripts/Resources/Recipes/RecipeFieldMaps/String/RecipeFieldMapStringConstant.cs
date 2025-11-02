using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeFieldMapStringConstant : RecipeFieldMapString {
    [Export] private string _value;
    public override string ResolveTemplate(Dictionary<string, Item> suppliedIngredients) {
        return _value;
    }
}