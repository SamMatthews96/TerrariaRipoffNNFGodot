using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeStatic : Recipe {
    [Export] private Item _resultItem;
    
    public override StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        IngredientProperty wood = GetIngredientType("wood", suppliedIngredients);
        if (wood is null) return null;
        return new StackedItems(_resultItem);
    }
}