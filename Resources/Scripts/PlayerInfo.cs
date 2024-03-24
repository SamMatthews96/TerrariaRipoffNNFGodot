using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class PlayerInfo : Resource {
    public string Name { get; private set; }

    public string UniqueName { get; private set; }

    public PlayerInfo(string uuid, string name) {
        Name = name;
        UniqueName = name + "-" + uuid;
    }
}