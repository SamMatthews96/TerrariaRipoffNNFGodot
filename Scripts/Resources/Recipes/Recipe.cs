using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class Recipe : Resource {
    [Export] private Array<RecipeIngredientSlot> _ingredientSlotArray;
    private Dictionary<string, RecipeIngredientSlot> _ingredientSlots;
    public Dictionary<string, RecipeIngredientSlot> IngredientSlots {
        get {
            if (_ingredientSlots is null) {
                _ingredientSlots = new Dictionary<string, RecipeIngredientSlot>();
                foreach (var craftingStationRecipes in _ingredientSlotArray) {
                    _ingredientSlots[craftingStationRecipes.RecipeSlot] = craftingStationRecipes;
                }
            }

            return _ingredientSlots;
        }
        private set => _ingredientSlots = value;
    }
    [Export] public Texture2D ResultIcon { get; private set; }
    [Export] public string Name { get; private set; }

    protected IngredientProperty GetIngredientType(string key, Dictionary<string, Item> suppliedIngredients) {
        if (!suppliedIngredients.TryGetValue(key, out Item item)) return null;
        return item.GetProperty<ItemIngredient>()
            .GetProperty(IngredientSlots[key].IngredientType);
    }

    public abstract StackedItems Build(Dictionary<string, Item> suppliedIngredients);
}