using System;
using Godot;
using Godot.Collections;
namespace TerrariaRipoffNNF;

public partial class WorldBasicInfo : Resource {
    public string Name { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public WorldBasicInfo(string name, int width, int height) {
        Name = name;
        Width = width;
        Height = height;
    }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("Width", Width);
        serializedData.Add("Height", Height);
        return serializedData;
    }

    public static WorldBasicInfo FromDict(Dictionary dictionary) {
        try {
            return new WorldBasicInfo(
                dictionary["Name"].ToString(),
                dictionary["Width"].ToString().ToInt(),
                dictionary["Height"].ToString().ToInt());
        }
        catch (Exception e) {
            GD.PrintErr("error reading WorldBasicInfo from dict");
            GD.PrintErr(e);
            throw new NotImplementedException();
        }
    }
}
