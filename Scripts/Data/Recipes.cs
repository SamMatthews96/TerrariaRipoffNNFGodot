// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// [GlobalClass][Tool]
// public partial class Recipes : Resource {
//     // private const string RecipeDir = "res://Resources/Recipes";
//
//     // [Export] private Dictionary<uint, Recipe> _recipes;
//     //
//     // public Array<Recipe> GetRecipes(CraftingStationType craftingStationType) {
//     //     Array<Recipe> recipes = new();
//     //     foreach (Recipe recipe in _recipes.Values) {
//     //         if (recipe.RequiredCraftingStation == craftingStationType) {
//     //             recipes.Add(recipe);
//     //         }
//     //     }
//     //
//     //     return recipes;
//     // }
//     
//     // public Recipe GetRecipe(uint id) {
//     //     return _recipes[id];
//     // }
//
//     // public void SetRecipes() {
//         // _recipes.Clear();
//         //
//         // DirAccess dirAccess = DirAccess.Open(RecipeDir);
//         // string[] resourceFiles = dirAccess.GetFiles();
//         // uint count = 0;
//         // foreach (string resourceFile in resourceFiles) {
//         //     if (!resourceFile.EndsWith(".tres")) continue;
//         //     string resourcePath = $"{RecipeDir}/{resourceFile}";
//         //     Recipe recipe = ResourceLoader.Load<Recipe>(resourcePath);
//         //     if (recipe is null) {
//         //         GD.PrintErr($"Failed to load recipe from {resourcePath}");
//         //         return;
//         //     }
//         //     recipe.Id = ++count;
//         //     _recipes.Add(count, recipe);
//         // }
//     // }
//     
// }