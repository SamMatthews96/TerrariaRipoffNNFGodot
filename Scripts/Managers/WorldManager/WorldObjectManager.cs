using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node {
    public static WorldObjectManager Create() {
        return Data.PackedScenes.WorldObjectManager.Instantiate<WorldObjectManager>();
    }
    
    private Array<SavedWorldObject>[,] savedWorldObjects;
    private Array<ActiveWorldObject>[,] activeWorldObjects;
    
}

