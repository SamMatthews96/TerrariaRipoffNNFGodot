using Godot;

[GlobalClass]
public partial class SavedBlock : Resource {
    public BlockType BlockType { get; private set; }
    

    public SavedBlock(BlockType blockType) {
        BlockType = blockType;
    }
    
}