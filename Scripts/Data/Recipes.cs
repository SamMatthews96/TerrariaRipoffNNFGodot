using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipes : Resource {
    private static string RecipesDir => "res://Resources/Recipes";

    private Array<Recipe> _recipes;

    public Array<Recipe> GetRecipes(CraftingStationType craftingStationType) {
        if (_recipes is null) {
            _recipes = new Array<Recipe>();
            Load();
        }

        Array<Recipe> recipes = new();
        foreach (Recipe recipe in _recipes) {
            if (recipe.RequiredCraftingStation == craftingStationType) {
                recipes.Add(recipe);
            }
        }

        return recipes;
    }

    private void Load() {
        DirAccess dirAccess = DirAccess.Open(RecipesDir);
        string[] resourceFiles = dirAccess.GetFiles();

        foreach (string resourceFile in resourceFiles) {
            string resourcePath = $"{RecipesDir}/{resourceFile}";
            Recipe newResource = ResourceLoader.Load<Recipe>(resourcePath);
            if (newResource != null) {
                _recipes.Add(newResource);
            } else {
                GD.PrintErr($"Failed to load recipe from {resourcePath}");
            }
        }
    }
}