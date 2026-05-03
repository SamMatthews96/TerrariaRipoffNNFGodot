using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropStationOutputTemplate : PropPropertyOutputTemplate {
    [Export] public CraftingStationType Type { get; private set; }
    
    public override PropProperty Build(Dictionary<string, Item> suppliedIngredients) {
        return PropStation.Create(Type);
    }
}