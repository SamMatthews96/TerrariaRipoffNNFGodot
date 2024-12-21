using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredient : ItemProperty {
    public override PropertyType PropertyType => PropertyType.Ingredient;
    [Export] private Array<IngredientProperty> _ingredientProperties = new();

    public override Dictionary ToDictionary() {
        throw new NotImplementedException();
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary newDictionary = new();
        foreach (IngredientProperty ingredientProperty in _ingredientProperties) {
            newDictionary.Add(ingredientProperty.IngredientType.ToString(),
                ingredientProperty.Quality);
        }

        return newDictionary;
    }

    public IngredientProperty GetProperty(IngredientType ingredientType) {
        return _ingredientProperties.First(property => property.IngredientType == ingredientType);
    }

    public bool HasProperty(IngredientType ingredientType) {
        return _ingredientProperties.Any(ingredientProperty =>
            ingredientProperty.IngredientType == ingredientType);
    }
}