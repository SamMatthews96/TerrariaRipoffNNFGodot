using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCrafted : ItemProperty {
    public Recipe Recipe { get; private set; }
    public Dictionary<string, Item> SuppliedIngredients { get; private set; }

    public static bool AreEqual(ItemCrafted a, ItemCrafted b) {
        if (a.Recipe != b.Recipe) {
            return false;
        }

        if (a.SuppliedIngredients.Count != b.SuppliedIngredients.Count) {
            return false;
        }

        foreach ((string key, Item aItem) in a.SuppliedIngredients) {
            Item bItem = b.SuppliedIngredients[key];
            if (!Item.AreEqual(aItem,bItem)) {
                return false;
            }
        }

        return true;
    }

    public ItemCrafted(Recipe recipe, Dictionary<string, Item> suppliedIngredients) {
        Recipe = recipe;
        SuppliedIngredients = suppliedIngredients;
    }

    public ItemCrafted() { }

    public Dictionary ToDictionary() {
        Dictionary newDictionary = new();
        newDictionary.Add("RecipeResourcePath", Recipe.ResourcePath);
        Dictionary suppliedIngredientsDict = new();
        foreach ((string key, Item item) in SuppliedIngredients) {
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