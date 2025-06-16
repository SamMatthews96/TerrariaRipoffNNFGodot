namespace TerrariaRipoffNNF;

public partial class ObjectPlacementCollision : ObjectProperty {
    public PlacementCollisionLayer Layer { get; private set; }

    public ObjectPlacementCollision(
        WorldObject worldObject, PlacementCollisionLayer layer) : base(worldObject) {
        Layer = layer;
    }

    public override void Init() {
        
    }
}