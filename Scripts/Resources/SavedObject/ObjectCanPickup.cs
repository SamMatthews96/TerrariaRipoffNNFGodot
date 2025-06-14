using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectCanPickup : ObjectProperty {
    [Export] public Item Item;
    public override void OnWorldObjectCreate(WorldObject worldObject) {

    }
}