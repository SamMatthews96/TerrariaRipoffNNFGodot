using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemMiningPropertyOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipePropertyMapMultiplier Speed { get; private set; }
    [Export] public RecipePropertyMapMultiplier Range { get; private set; }
    [Export] public RecipePropertyMapMultiplier Power { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        return ItemMining.Create(
            speed: Speed.ResolveTemplate(suppliedIngredients, ingredientSlots),
            range: Range.ResolveTemplate(suppliedIngredients, ingredientSlots),
            power: Power.ResolveTemplate(suppliedIngredients, ingredientSlots)
        );
    }
}