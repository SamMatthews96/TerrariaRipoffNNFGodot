using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientMouseover : TextureRect {
    private Item _selectedIngredientItem;
    private RecipeIngredientSlot _recipeIngredientSlot;
    [Export] private Label _quantityLabel;

    public event Action<Control, RecipeIngredientSlot> MouseEnteredIcon;
    public event Action MouseLeftIcon;


    public static SelectIngredientMouseover Create(RecipeIngredientSlot ingredient) {
        SelectIngredientMouseover newTexture =
            Data.PackedScenes.RecipeIngredientSlotTexture
                .Instantiate<SelectIngredientMouseover>();
        newTexture._recipeIngredientSlot = ingredient;
        newTexture.Texture = ingredient.Icon;
        newTexture._quantityLabel.Text = ingredient.Amount.ToString();
        return newTexture;
    }

    public override void _Ready() {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    public override void _ExitTree() {
        MouseEntered -= OnMouseEntered;
        MouseExited -= OnMouseExited;
    }

    private void OnMouseEntered() {
        MouseEnteredIcon?.Invoke(this, _recipeIngredientSlot);
    }

    private void OnMouseExited() {
        MouseLeftIcon?.Invoke();
    }
}