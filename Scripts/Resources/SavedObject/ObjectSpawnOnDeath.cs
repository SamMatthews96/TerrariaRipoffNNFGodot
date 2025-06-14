using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectSpawnOnDeath : ObjectProperty {
    [Export] public SavedObject SavedObject { get; private set; }
    
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        
    }
}