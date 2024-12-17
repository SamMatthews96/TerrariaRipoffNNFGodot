using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PackedScenes : Resource {
    [Export] public PackedScene Player { get; private set; }
    [Export] public PackedScene ActiveBlock { get; private set; }
    [Export] public PackedScene ActivePickup { get; private set; }
    [Export] public PackedScene MainMenu { get; private set; }
    [Export] public PackedScene LoadScreen { get; private set; }
    [Export] public PackedScene Game { get; private set; }
    [Export] public PackedScene SelectCraftingStationButton { get; private set; }
}