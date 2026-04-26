using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class IngredientMouseover : TextureRect {
    [Export] private Label _quantityLabel;
    
    public static IngredientMouseover Create(Ingredient ingredient) {
        IngredientMouseover newTexture =
            Data.PackedScenes.RecipeIngredientSlotTexture
                .Instantiate<IngredientMouseover>();
        newTexture.Texture = ingredient.Icon;
        newTexture._quantityLabel.Text = ingredient.Amount.ToString();
        return newTexture;
    }
}