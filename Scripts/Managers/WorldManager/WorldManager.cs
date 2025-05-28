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

    /* 
        currently the different gameObjects: Blocks, Pickups, Placeables (furnance workbench etc)
        are handled by their own classes. But they all need to implement the properties/methods:
            Position
            Spawn when player is close
      
        Idea: add a WorldObject class with abstract/implementations
        

    */

}