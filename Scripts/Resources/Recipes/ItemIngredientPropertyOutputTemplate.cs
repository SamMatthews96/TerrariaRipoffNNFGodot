using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredientPropertyOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public RecipePropertyMapMultiplier Quality { get; private set; }
    [Export] public RecipePropertyMapString Name { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        return ItemIngredient.Create(
            type: IngredientType,
            quality: Quality.ResolveTemplate(suppliedIngredients, ingredientSlots),
            name: Name.ResolveTemplate(suppliedIngredients, ingredientSlots)
        );
    }
}