using Godot;

namespace TerrariaRipoffNNF;

public partial class Host : Node {
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }

}