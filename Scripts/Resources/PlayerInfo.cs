using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class PlayerInfo : Resource {
    public string Name { get; private set; }

    public string UniqueName { get; private set; }

    public PlayerInfo() { }
    
    public PlayerInfo(string uuid, string name) {
        Name = name;
        UniqueName = name + "-" + uuid;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "Name", Name },
            { "UniqueName", UniqueName }
        };
    }

    public static PlayerInfo FromDict(Dictionary dictionary) {
        try {
            return new PlayerInfo(
                dictionary["Name"].ToString(),
                dictionary["UniqueName"].ToString()
            );
        } catch (Exception e) {
            GD.PrintErr("Error deserializing PlayerInfo: " + e.Message);
            throw new NotImplementedException();
        }
    }
}