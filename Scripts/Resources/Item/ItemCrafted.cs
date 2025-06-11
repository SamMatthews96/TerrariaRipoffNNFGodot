using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCrafted : ItemProperty {
    private Recipe _recipe;
    private Dictionary<string, Item> _suppliedIngredients;
    
    public ItemCrafted(Recipe recipe, Dictionary<string, Item> suppliedIngredients) {
        _recipe = recipe;
        _suppliedIngredients = suppliedIngredients;
    }

    public ItemCrafted() { }

    public Dictionary ToDictionary() {
        Dictionary newDictionary = new();
        newDictionary.Add("RecipeResourcePath", _recipe.ResourcePath);
        Dictionary suppliedIngredientsDict = new();
        foreach ((string key, Item item) in _suppliedIngredients) {
            suppliedIngredientsDict.Add(key, item.ToDictionary());
        }
        newDictionary.Add("SuppliedIngredients", suppliedIngredientsDict);
        return newDictionary;
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Block");
        return tooltipAttributes;
    }
}