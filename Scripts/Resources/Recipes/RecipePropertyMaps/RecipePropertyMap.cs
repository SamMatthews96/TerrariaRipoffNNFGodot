using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class RecipePropertyMap<T> : Resource {
    public abstract T ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots);
}