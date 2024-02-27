using Godot;

[GlobalClass]
public partial class SavedBlock : Resource {
    public BlockType BlockType { get; private set; }
    public int XPosition { get; private set; }
    public int YPosition { get; private set; }

    public SavedBlock(BlockType blockType, int xPosition, int yPosition) {
        BlockType = blockType;
        XPosition = xPosition;
        YPosition = yPosition;
    }
    
    
    
    /*
     *  when savedBlock updated, (created, deleted, updated health)
     *      get players who are close
     *      create:
     *      rpc them, create activeBlock
     *      update:
     *      rpc them, update activeBlock
     *      delete:
     *      rpc them, delete activeBlock
     *
     *  when player tries to create Block
     *      rpc server: validate, create SavedBlock, emit savedBlockCreated
     *  when player tries to update Block
     *      rpc server: validate, update SavedBlock, emit savedBlockUpdated
     *  when player tries to delete Block
     *      rpc server: validate, delete SavedBlock, emit savedBlockDeleted
     *  server is source of truth
     *
     */
}