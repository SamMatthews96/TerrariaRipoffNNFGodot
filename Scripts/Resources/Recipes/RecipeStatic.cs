using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeStatic : Recipe {
    [Export] private Item _resultItem;

    public override StackedItems Build(Dictionary<string, Item> suppliedIngredients) {
        foreach (string key in IngredientSlots.Keys) {
            IngredientProperty ingredientProperty = GetIngredientType(key, suppliedIngredients);
            if (ingredientProperty is null) return null;
        }

        return new StackedItems(_resultItem);
    }
}