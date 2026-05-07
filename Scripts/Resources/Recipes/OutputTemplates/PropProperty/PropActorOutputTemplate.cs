using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropActorOutputTemplate : PropPropertyOutputTemplate {
    [Export] public PackedScene ActorScene { get; private set; }
    public override PropProperty Build(Dictionary<string, Item> suppliedIngredients) {
        return PropActor.Create(ActorScene);
    }
    
}