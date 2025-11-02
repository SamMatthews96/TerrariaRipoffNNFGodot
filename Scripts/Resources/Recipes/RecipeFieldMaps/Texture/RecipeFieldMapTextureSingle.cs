using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeFieldMapTextureSingle : RecipeFieldMapTexture {
    [Export] private string _ingredientName;
    [Export] private IngredientToOutputTextureMap _ingredientToTextureMap;

    public override Texture2D ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients) {
        Item ingredient = suppliedIngredients[_ingredientName];
        ItemIngredient itemIngredient = ingredient.GetProperty<ItemIngredient>();
        Texture2D texture = _ingredientToTextureMap.Map[itemIngredient];
        return texture;
    }
}