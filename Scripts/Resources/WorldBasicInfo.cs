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

    // ReSharper disable once UnusedMember.Global
    public WorldBasicInfo() { }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("Name", Name);
        serializedData.Add("Width", Width);
        serializedData.Add("Height", Height);
        return serializedData;
    }

    public static WorldBasicInfo FromDictionary(Dictionary dictionary) {
        return new WorldBasicInfo(
            dictionary["Name"].ToString(),
            (int)Math.Round(dictionary["Width"].ToString().ToFloat()),
            (int)Math.Round(dictionary["Height"].ToString().ToFloat()));
    }
}