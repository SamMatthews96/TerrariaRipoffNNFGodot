using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Recipes;

[GlobalClass]
public partial class RecipeMining : Resource {
    public Array<RecipeIngredientProperty> RequiredIngredients { get; private set; }
    public Dictionary<string, int> temp;
    
    // and for each optional ingredientGroup
    // array <ingredientProperty, suppliedIngredient, effect>

    public void Build() {
        Item.Builder builder = Item.CreateFromRecipe();
        // get the metal property
        RecipeIngredientProperty metalProperty = RequiredIngredients[0];
        float quality = metalProperty.SelectedItem.GetProperty<ItemIngredient>()
            .GetProperty(IngredientType.StrongMetal).Quality;
        string ingredientName = metalProperty.SelectedItem.ResourceName;
        
        
    }
}
