using Godot;

namespace TerrariaRipoffNNF;

public partial class PackedScenes : Node {
    [Export] public PackedScene PackedPlayer { get; private set; }
    [Export] public PackedScene PackedBlock { get; private set; }
    [Export] public PackedScene PackedPickup { get; private set; }
    [Export] public PackedScene PackedMainMenu { get; private set; }
}