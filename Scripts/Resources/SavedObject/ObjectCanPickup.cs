namespace TerrariaRipoffNNF;

public partial class ObjectCanPickup : ObjectProperty {
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        // instructing the worldObject to obey gravity, and have a pickup radius
        // need we override the Data.PackedScenes.WorldObject
    }
}