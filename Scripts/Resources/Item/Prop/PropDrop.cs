using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropDrop : PropProperty {
    [Export] public Item Item { get; private set; }
}