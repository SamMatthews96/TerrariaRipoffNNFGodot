using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredientPropertyOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public ItemIngredient Output { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        return Output;
    }
}