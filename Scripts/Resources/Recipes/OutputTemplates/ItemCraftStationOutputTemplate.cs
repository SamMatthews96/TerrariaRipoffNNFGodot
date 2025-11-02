using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCraftStationOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public CraftingStationType Type { get; private set; }
    
    public override ItemProperty Build(Dictionary<string, Item> suppliedIngredients) {
        return new ItemCraftStation(Type);
    }
}