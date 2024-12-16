using Godot;

namespace TerrariaRipoffNNF;

public partial class PackedScenes : Node {
    // @todo neater way to implement this?
    // nodes are, definitionally part of a scene,
    // so perhaps these should belong to a resource instead.
    [Export] public PackedScene PackedPlayer { get; private set; }
    [Export] public PackedScene PackedBlock { get; private set; }
    [Export] public PackedScene PackedPickup { get; private set; }
    [Export] public PackedScene PackedMainMenu { get; private set; }
    [Export] public PackedScene PackedLoadScreen { get; private set; }
    [Export] public PackedScene PackedGame { get; private set; }
    [Export] public PackedScene SelectCraftingStationButton { get; private set; }
}