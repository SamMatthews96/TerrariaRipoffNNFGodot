
using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class WorldBasicInfo : Resource {
    public string Name { get; private set; }
    public int WorldWidth { get; private set; }
    public int WorldHeight { get; private set; }
}
