using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientButton : TextureButton {
    public static SelectIngredientButton Create(Item item) {
        SelectIngredientButton selectIngredientButton =
            Manager.Instance.PackedScenes.SelectIngredientButton
                .Instantiate<SelectIngredientButton>();
        selectIngredientButton.TextureNormal = item.IconTexture;
        return selectIngredientButton;
    }
}