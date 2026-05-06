using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class PropProperty : Resource {
    public virtual void Apply(ActiveProp prop, World world) { }
}