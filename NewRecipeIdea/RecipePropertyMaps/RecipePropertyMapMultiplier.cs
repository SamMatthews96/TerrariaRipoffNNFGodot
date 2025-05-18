using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipePropertyMapMultiplier : RecipePropertyMap<float> {
    [Export] private float _base;
    [Export] private float _multiplier;
    [Export] private string _ingredientName;

    public override float ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients,
        Array<RecipeIngredientSlot> ingredientSlots
    ) {
        RecipeIngredientSlot ingredientSlot = ingredientSlots.First(
            ingredient => ingredient.RecipeSlot == _ingredientName);

        float suppliedItemQuality = suppliedIngredients[_ingredientName]
            .GetProperty<ItemIngredient>()
            .GetProperty(ingredientSlot.IngredientType).Quality;

        return _base + suppliedItemQuality * _multiplier;
    }
}