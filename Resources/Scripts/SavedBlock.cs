using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.scripts;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class SavedBlock : Resource, ISerializable {
    public int XPosition;
    public int YPosition;
    public BlockType BlockType;

    public SavedBlock(BlockType blockType, int xPosition, int yPosition) {
        BlockType = blockType;
        XPosition = xPosition;
        YPosition = yPosition;
    }
    
    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("XPosition", XPosition);
        serializedData.Add("YPosition", YPosition);
        serializedData.Add("ResourcePath", BlockType.ResourcePath);
        return serializedData;
    }

    public static SavedBlock FromDict(Dictionary dictionary) {
        return new SavedBlock(
            ResourceLoader.Load<BlockType>(dictionary["ResourcePath"].ToString()),
            dictionary["XPosition"].ToString().ToInt(),
            dictionary["YPosition"].ToString().ToInt()
            );
    }
}