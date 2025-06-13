using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectCollision : ObjectProperty {
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        StaticBody2D staticBody =
            Data.PackedScenes.WorldSolid.Instantiate<StaticBody2D>();
        worldObject.AddChild(staticBody);
    }
}