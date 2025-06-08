using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipes : Resource {
    [Export] private Array<Recipe> _recipes = new();
    
    public Array<Recipe> GetRecipes(CraftingStationType craftingStationType) {
        Array<Recipe> recipes = new();
        foreach (Recipe recipe in _recipes) {
            if (recipe.CraftingStationType == craftingStationType) {
                recipes.Add(recipe);
            }
        }
        return recipes;
    }
}