using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class ActiveWorldObject : Node {
    public SavedWorldObject SavedWorldObject { get; private set; }
}