using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientButton : TextureButton {
    private Item _item;
    public static SelectIngredientButton Create(Item item) {
        SelectIngredientButton selectIngredientButton =
            Data.PackedScenes.SelectIngredientButton
                .Instantiate<SelectIngredientButton>();
        selectIngredientButton.TextureNormal = item.IconTexture;
        selectIngredientButton._item = item;
        return selectIngredientButton;
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