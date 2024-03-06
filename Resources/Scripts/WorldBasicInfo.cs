
using System;
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

    public static WorldBasicInfo FromDict(Dictionary dictionary) {
        try {
            return new WorldBasicInfo(
                dictionary["Name"].ToString(),
                dictionary["WorldWidth"].ToString().ToInt(),
                dictionary["WorldHeight"].ToString().ToInt());
        }
        catch (Exception e) {
            GD.Print("invalid WorldBasicInfo dict");
            GD.Print(e);
            throw new NotImplementedException();
        }
    }
}
