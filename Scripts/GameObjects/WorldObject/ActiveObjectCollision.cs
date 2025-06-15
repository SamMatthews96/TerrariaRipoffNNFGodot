using Godot;

namespace TerrariaRipoffNNF;

public class ActiveObjectCollision : ActiveObjectProperty {
    public ActiveObjectCollision(WorldObject worldObject) : base(worldObject) { }
    public override void Init() {
        StaticBody2D staticBody =
            Data.PackedScenes.WorldSolid.Instantiate<StaticBody2D>();
        WorldObject.ParentNode.AddChild(staticBody);
    }
}