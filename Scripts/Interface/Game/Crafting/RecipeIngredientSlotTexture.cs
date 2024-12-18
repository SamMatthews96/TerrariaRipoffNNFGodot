using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class RecipeIngredientSlotTexture : TextureRect {
    private Item _selectedIngredientItem;
    private RecipeIngredientSlot _ingredient;
    
    public event Action<Vector2, RecipeIngredientSlot> MouseEnteredIcon;
    
    public static RecipeIngredientSlotTexture Create(RecipeIngredientSlot ingredient) {
        RecipeIngredientSlotTexture newTexture =
            Manager.Instance.PackedScenes.RecipeIngredientSlotTexture
                .Instantiate<RecipeIngredientSlotTexture>();
        newTexture._ingredient = ingredient;
        newTexture.Texture = ingredient.Icon;
        return newTexture;
    }

    public override void _Ready() {
        MouseEntered += OnMouseEntered;
        MouseExited += () => {
            // _ingredientPopupPanel.Hide();
        };

    }

    private void OnMouseEntered() {
        MouseEnteredIcon?.Invoke(GlobalPosition + new Vector2(Size.X / 2, 0), _ingredient);
    }
}       
        
