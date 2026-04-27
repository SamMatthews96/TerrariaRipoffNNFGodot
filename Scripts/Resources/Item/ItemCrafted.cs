using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCrafted : ItemProperty {
    // private Recipe _recipe;
    private Dictionary<string, Item> _suppliedIngredients;

    public static bool AreEqual(ItemCrafted a, ItemCrafted b) {
        // if (a._recipe != b._recipe) {
        //     return false;
        // }

        if (a._suppliedIngredients.Count != b._suppliedIngredients.Count) {
            return false;
        }

        foreach ((string key, Item aItem) in a._suppliedIngredients) {
            Item bItem = b._suppliedIngredients[key];
            if (!Item.AreEqual(aItem,bItem)) {
                return false;
            }
        }

        return true;
    }

    // public ItemCrafted(Recipe recipe, Dictionary<string, Item> suppliedIngredients) {
    //     _recipe = recipe;
    //     _suppliedIngredients = suppliedIngredients;
    // }

    public ItemCrafted() { }

    public Dictionary ToDictionary() {
        Dictionary newDictionary = new();
        // newDictionary.Add("RecipeResourcePath", _recipe.ResourcePath);
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