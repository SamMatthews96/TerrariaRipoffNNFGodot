using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class RecipeIngredientSlotTexture : TextureRect {
    private Item _selectedIngredientItem;
    private RecipeIngredientSlot _ingredient;

    public event Action<Control, IngredientType> MouseEnteredIcon;
    public event Action MouseLeftIcon;


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
        MouseExited += OnMouseExited;
    }
    
    public override void _ExitTree() {
        MouseEntered -= OnMouseEntered;
        MouseExited -= OnMouseExited;
    }

    private void OnMouseEntered() {
        MouseEnteredIcon?.Invoke(this, _ingredient.IngredientType);
    }

    private void OnMouseExited() {
        MouseLeftIcon?.Invoke();
    }
}