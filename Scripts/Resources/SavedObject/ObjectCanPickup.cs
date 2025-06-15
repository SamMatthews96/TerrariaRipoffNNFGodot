using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectCanPickup : ObjectProperty {
    [Export] public Item Item { get; private set; }
    public override void Register(WorldObject worldObject) {
        worldObject.ParentNode = WorldPickup.Create(Item, worldObject);
        worldObject.ActiveProperties
            .Add(new ActiveObjectCanPickup(worldObject));
    }
}