using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass][Tool]
public partial class Recipes : Resource {
    private const string RecipeDir = "res://Resources/Recipes";

    private Array<Recipe> _recipeList;

    public Recipes() {
        _recipeList = new Array<Recipe>();
        DirAccess dirAccess = DirAccess.Open(RecipeDir);
        string[] resourcePaths = dirAccess.GetFiles();
        foreach (string fileName in resourcePaths) {
            string resourcePath = $"{RecipeDir}/{fileName}";            
            Recipe recipe = ResourceLoader.Load<Recipe>(resourcePath);
            _recipeList.Add(recipe);
        }
    }

    public Array<Recipe> GetRecipes(StationType stationType) {
        Array<Recipe> recipes = new();
        int count = 0;
        foreach (Recipe recipe in _recipeList) {
            recipe.Id = count;
            count++;
            if (recipe.RequiredStation == stationType) {
                recipes.Add(recipe);
            }
        }
    
        return recipes;
    }
    
    public Recipe GetRecipe(int id) {
        return _recipeList[id];
    }




}