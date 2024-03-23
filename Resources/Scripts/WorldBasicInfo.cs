
using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class WorldBasicInfo : Resource {
    public string Name { get; private set; }
    public int WorldWidth { get; private set; }
    public int WorldHeight { get; private set; }
    
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

    public static WorldBasicInfo FromDict(Dictionary dictionary) {
        try {
            return new WorldBasicInfo(
                dictionary["Name"].ToString(),
                dictionary["WorldWidth"].ToString().ToInt(),
                dictionary["WorldHeight"].ToString().ToInt());
        }
        catch (Exception e) {
            GD.Print("error reading WorldBasicInfo from dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
