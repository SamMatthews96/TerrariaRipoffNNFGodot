using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class SavedBlock : Resource, ISerializable {
    private int xPosition;
    private int yPosition;
    private BlockType blockType;

    public SavedBlock(BlockType blockType, int xPosition, int yPosition) {
        this.blockType = blockType;
        this.xPosition = xPosition;
        this.yPosition = yPosition;
    }
    
    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("XPosition", xPosition);
        serializedData.Add("YPosition", yPosition);
        serializedData.Add("BlockTypeId", blockType.GetInstanceId().ToString());
        return serializedData;
    }
}