using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class ActiveWorldObject : Node2D {
    public SavedWorldObject SavedWorldObject { get; private set; }
}