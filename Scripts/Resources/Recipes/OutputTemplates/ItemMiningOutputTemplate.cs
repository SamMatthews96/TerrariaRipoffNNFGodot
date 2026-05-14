using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemMiningOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipeFieldMapFloat Speed { get; private set; }
    [Export] public RecipeFieldMapFloat Range { get; private set; }
    [Export] public RecipeFieldMapFloat Power { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        throw new NotImplementedException();
    }
}