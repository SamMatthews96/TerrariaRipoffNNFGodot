using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class ActiveWorldObject : Node2D {
    public SavedWorldObject SavedWorldObject { get; protected set; }
    // public SavedWorldObject 
    protected Dictionary ObjectConfig;

    
    
}