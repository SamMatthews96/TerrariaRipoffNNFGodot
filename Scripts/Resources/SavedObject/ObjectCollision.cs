using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectCollision : ObjectProperty {
    public override void Register(WorldObject worldObject) {
        worldObject.ActiveProperties
            .Add(new ActiveObjectCollision(worldObject));
    }
}