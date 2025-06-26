using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Items : Resource {
    [Export] public Item Stone { get; private set; }
    [Export] public Item Earth { get; private set; }
    [Export] public Item IronOre { get; private set; }
    [Export] public Item Wood { get; private set; }
}