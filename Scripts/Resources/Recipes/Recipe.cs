using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Recipes;

[GlobalClass]
public abstract partial class Recipe : Resource {
    [Export] public Dictionary<string, RecipeIngredientSlot> Ingredients;
    protected Dictionary<string, Item> SuppliedIngredients;

    protected IngredientProperty GetIngredientType(string key) {
        return SuppliedIngredients[key]
            .GetProperty<ItemIngredient>()
            .GetProperty(Ingredients[key].IngredientType);
    }

    public abstract Item Build(Dictionary<string, Item> suppliedIngredients);
}