using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectPickup : ObjectProperty {
    [Export] public Item Item { get; private set; }
    
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        // easy to listen to events from other properties
        
    }
}