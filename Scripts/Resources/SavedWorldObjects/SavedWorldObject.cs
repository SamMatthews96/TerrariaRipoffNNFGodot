using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class SavedWorldObject : Resource {
    public int XPosition { get; protected set; }
    public int YPosition { get; protected set; }
    

    public abstract Dictionary ToDictionary();

    // Create can just be defined on the child classes

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