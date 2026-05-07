using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropTriggerOutputTemplate : PropPropertyOutputTemplate {
    [Export] public PackedScene TriggerScene { get; private set; }
    public override PropProperty Build(Dictionary<string, Item> suppliedIngredients) {
        return PropTrigger.Create(TriggerScene);
    }
}