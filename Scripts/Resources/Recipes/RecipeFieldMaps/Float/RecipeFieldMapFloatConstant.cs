using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeFieldMapFloatConstant : RecipeFieldMapFloat {
    [Export] private float _value;

    public override float ResolveTemplate(Dictionary<string, Item> suppliedIngredients) {
        return _value;
    }
}