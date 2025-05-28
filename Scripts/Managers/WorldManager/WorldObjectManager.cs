using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObjectManager : Node
{
  public static WorldManager Create()
  {
    return Data.PackedScenes.WorldManager.Instantiate<WorldObjectManager>();
  }


  private Array<SavedWorldObject>[,] savedWorldObjects;
  private Array<ActiveWorldObject>[,] activeWorldObjects;

  private void Test()
  {

  }

  
}

public abstract partial class SavedWorldObject : Resource
{

  // IntVector isn't a variant type
  // if we forget IntVectors life may just get easier
  public int XPosition { get; private set; }
  public int YPosition { get; private set; }

  public abstract Dictionary ToDictionary();

  // Create can just be defined on the child classes


  // Positions were init on SavedBlock
  // I think it should be private set for SavedWorldObject
  // worldObjects can move in case of fluids or pickups

  // would it be insane to use composition for worldObjects?
  // Examples of WorldObjects:
  // Wall, Block, Placeable, Fluid, Pickup
  /* all have position
    the display rules of ActiveObjects may differ, 
    but have rules in common
    some may need to move
      moving will have to relocate a savedObjects position,
      meaning a
  */
  /* 
    Ultimately, I think it should use inheritance,
    for the reason that each type needs to be recognisable
    as such
    on the other hand, walls and blocks are examples where
    in certain contexts they act as the same type. 
    Walls blocks and placeables are static objects.
    Perhaps some degree of composition will benefit.
    Fluids, characters and pickups need to move
    Default spawning rules will be if a player is within
    viewing range. 

  */

  /* But what about objects that occupy multiple cells,
    i.e. placeables
    Each cell inside of WorldObjectManager needs to be able 
    to return information about everything that is in its cell.
    Placeables could exist as Placeable(component) or 
    Placeable(root/main??) 
    Components will need to hold a reference to the main, and
    when a component receives an action(gather) then that
    signal is forwarded to the main.
    Likewise, actions performed on the main may need to be 
    perpetuated to all components. eg. When the main is 
    destroyed, all components should be deleted.
  */

}

public abstract partial class ActiveWorldObject : Node
{
  public SavedWorldObject SavedWorldObject { get; private set; }
  
}