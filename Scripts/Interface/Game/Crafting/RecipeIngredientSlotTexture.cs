using Godot;

namespace TerrariaRipoffNNF;

public partial class RecipeIngredientSlotTexture : TextureRect {
    
    
    public static RecipeIngredientSlotTexture Create(RecipeIngredientSlot ingredient) {
        RecipeIngredientSlotTexture newTexture =
            Manager.Instance.PackedScenes.RecipeIngredientSlotTexture
                .Instantiate<RecipeIngredientSlotTexture>();
        
        newTexture.Texture = ingredient.Icon;
        return newTexture;
    }
}