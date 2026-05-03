using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class IngredientButton : TextureButton {
    private Item _item;
    public static IngredientButton Create(Item item) {
        IngredientButton ingredientButton =
            Data.PackedScenes.SelectIngredientButton
                .Instantiate<IngredientButton>();
        ingredientButton.TextureNormal = item.IconTexture;
        ingredientButton._item = item;
        return ingredientButton;
    }
    
    public event Action<Item> IngredientButtonClicked;

    public override void _Ready() {
        ButtonDown += OnButtonDown;
    }

    public override void _ExitTree() {
        ButtonDown -= OnButtonDown;
    }
    
    private void OnButtonDown() {
        IngredientButtonClicked?.Invoke(_item);
    }
}