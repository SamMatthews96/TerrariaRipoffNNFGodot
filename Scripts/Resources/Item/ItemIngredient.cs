using System;
using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredient : ItemProperty {
    [Export] private Array<IngredientProperty> _ingredientProperties = new();
    
    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Ingredient");
        foreach (IngredientProperty ingredientProperty in _ingredientProperties) {
            tooltipAttributes.Add(ingredientProperty.IngredientType.ToString(),
                ingredientProperty.Quality);
        }

        return tooltipAttributes;
    }

    public IngredientProperty GetProperty(IngredientType ingredientType) {
        return _ingredientProperties.First(property => property.IngredientType == ingredientType);
    }

    public bool HasProperty(IngredientType ingredientType) {
        return _ingredientProperties.Any(ingredientProperty =>
            ingredientProperty.IngredientType == ingredientType);
    }
}