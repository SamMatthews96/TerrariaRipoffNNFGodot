
using Godot;
using Godot.Collections;
using ISerializable = TerrariaRipoffNNF.scripts.ISerializable;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class WorldBasicInfo : Resource, ISerializable {
    public string Name { get; protected set; }
    public int WorldWidth { get; protected set; }
    public int WorldHeight { get; protected set; }
    
    public WorldBasicInfo(string name, int worldWidth, int worldHeight) {
        Name = name;
        WorldWidth = worldWidth;
        WorldHeight = worldHeight;
    }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("WorldWidth", WorldWidth);
        serializedData.Add("WorldHeight", WorldHeight);
        return serializedData;
    }
}
