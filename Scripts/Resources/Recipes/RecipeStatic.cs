using System.ComponentModel;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeStatic : Recipe {
    [Export] private Item _resultItem;
    
    public override Item Build(Dictionary<string, Item> suppliedIngredients) {
        return _resultItem;
    }
}