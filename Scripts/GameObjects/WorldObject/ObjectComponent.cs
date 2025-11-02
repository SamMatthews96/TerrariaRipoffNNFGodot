namespace TerrariaRipoffNNF;

public class ObjectComponent : ObjectProperty {
    private readonly WorldObject _main;
    public ObjectComponent(WorldObject worldObject, WorldObject main)
        : base(worldObject) {
        _main = main;
        
    }
    
    public override void Init() {
        _main.AddComponent(WorldObject);
    }
}