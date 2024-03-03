
using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class WorldBasicInfo : Resource {
    public string Name { get; protected set; }
    public int WorldWidth { get; protected set; }
    public int WorldHeight { get; protected set; }
    
}
