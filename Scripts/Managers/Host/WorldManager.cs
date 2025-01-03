using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldManager : Node {
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }

}