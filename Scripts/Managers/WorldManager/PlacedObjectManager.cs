using Godot;

namespace TerrariaRipoffNNF;

public partial class PlacedObjectManager : Node {
    /* Handles placement of non block objects like
     Furniture, torches, crafting stations, etc
     
     Will need to listen to player actions for placing and mining objects
     Placed objects will need coordinates and size
     When trying to place, this will need to check with the blockManager to see if 
     any blocks are in the way.
     */
    [Export] private BlockManager _blockManager;
    private SavedPlacedObject[,] _savedPlacedObjects;
    
}