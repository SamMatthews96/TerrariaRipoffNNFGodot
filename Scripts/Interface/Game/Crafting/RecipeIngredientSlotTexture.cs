using Godot;

namespace TerrariaRipoffNNF;

public partial class RecipeIngredientSlotTexture : TextureRect {
    private Item _selectedIngredientItem;
    [Export] private PopupPanel _ingredientPopupPanel;
    
    public static RecipeIngredientSlotTexture Create(RecipeIngredientSlot ingredient) {
        RecipeIngredientSlotTexture newTexture =
            Manager.Instance.PackedScenes.RecipeIngredientSlotTexture
                .Instantiate<RecipeIngredientSlotTexture>();
        
        newTexture.Texture = ingredient.Icon;
        return newTexture;
    }

    public override void _Ready() {
        _ingredientPopupPanel.Hide();
        MouseEntered += () => {
            // _ingredientPopupPanel.Show();
        };
        MouseExited += () => {
            // _ingredientPopupPanel.Hide();
        };

    }
}