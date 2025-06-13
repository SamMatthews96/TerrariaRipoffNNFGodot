using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ObjectProperty : Resource {
    public abstract void OnWorldObjectCreate(WorldObject worldObject);
}