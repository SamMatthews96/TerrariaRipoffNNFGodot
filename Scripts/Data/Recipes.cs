using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass][Tool]
public partial class Recipes : Resource {
    private const string RecipeDir = "res://Resources/Recipes";

    private Dictionary<int, Recipe> _recipeList = new();

    public Recipes() {
        DirAccess dirAccess = DirAccess.Open(RecipeDir);
        string[] resourcePaths = dirAccess.GetFiles();
        
        foreach (string fileName in resourcePaths) {
            string resourcePath = $"{RecipeDir}/{fileName}";            
            Recipe recipe = ResourceLoader.Load<Recipe>(resourcePath);
            _recipeList.Add(recipe.Id, recipe);
        }
    }

    public Array<Recipe> GetRecipes(StationType stationType) {
        Array<Recipe> recipes = new();
        foreach (Recipe recipe in _recipeList.Values) {
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