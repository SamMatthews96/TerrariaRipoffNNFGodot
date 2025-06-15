using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectCanPickup : ObjectProperty {
    [Export] public Item Item;
    public override void Register(WorldObject worldObject) {
        worldObject.ParentNode = 
            Data.PackedScenes.WorldPickup.Instantiate<Node2D>();
        worldObject.ActiveProperties
            .Add(new ActiveObjectCanPickup(worldObject));
    }
}