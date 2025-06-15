namespace TerrariaRipoffNNF;

public abstract class ActiveObjectProperty {
    protected WorldObject WorldObject { get; private set; }

    protected ActiveObjectProperty(WorldObject worldObject) {
        WorldObject = worldObject;
    }

    public abstract void Init();
}

