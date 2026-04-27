// using System;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// [GlobalClass]
// public partial class RecipeFieldMapMultiplier : RecipeFieldMapFloat {
//     [Export] private float _base;
//     [Export] private float _multiplier;
//     [Export] private string _ingredientName;
//
//     public override float ResolveTemplate(
//         Dictionary<string, Item> suppliedIngredients
//     ) {
//         if (_multiplier == 0) {
//             return _base;
//         }
//
//         if (_ingredientName == "") {
//             throw new Exception("[20250520.2338.1] Ingredient name is empty with a non-zero multiplier");
//         }
//
//         float suppliedItemQuality = suppliedIngredients[_ingredientName]
//             .GetProperty<ItemIngredient>().Quality;
//
//         return _base + suppliedItemQuality * _multiplier;
//     }
// }