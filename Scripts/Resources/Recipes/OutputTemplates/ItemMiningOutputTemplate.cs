using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemMiningOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipeFieldMapFloat Speed { get; private set; }
    [Export] public RecipeFieldMapFloat Range { get; private set; }
    [Export] public RecipeFieldMapFloat Power { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        return ItemMining.Create(
            speed: Speed.ResolveTemplate(suppliedIngredients),
            range: Range.ResolveTemplate(suppliedIngredients),
            power: Power.ResolveTemplate(suppliedIngredients)
        );
    }
}