using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredientOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public ItemIngredient Output { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {        
        throw new NotImplementedException();
    }
}