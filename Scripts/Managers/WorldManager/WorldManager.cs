using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldManager : Node {
    public static WorldManager Create() {
        return Data.PackedScenes.WorldManager.Instantiate<WorldManager>();
    }
    
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlaceableManager PlaceableManager { get; private set; }

}