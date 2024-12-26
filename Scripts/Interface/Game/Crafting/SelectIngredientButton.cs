using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientButton : TextureButton {
    private Item _item;
    public static SelectIngredientButton Create(Item item) {
        SelectIngredientButton selectIngredientButton =
            Manager.Instance.PackedScenes.SelectIngredientButton
                .Instantiate<SelectIngredientButton>();
        selectIngredientButton.TextureNormal = item.IconTexture;
        selectIngredientButton._item = item;
        return selectIngredientButton;
    }
    
    public event Action<Item> IngredientSelected;

    public override void _Ready() {
        ButtonDown += OnButtonDown;
    }

    public override void _ExitTree() {
        ButtonDown -= OnButtonDown;
    }
    
    private void OnButtonDown() {
        IngredientSelected?.Invoke(_item);
    }
}