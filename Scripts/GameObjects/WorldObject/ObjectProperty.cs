namespace TerrariaRipoffNNF;

public abstract class ObjectProperty {
    protected WorldObject WorldObject { get; private set; }

    protected ObjectProperty(WorldObject worldObject) {
        WorldObject = worldObject;
    }

    public abstract void Init();
}

