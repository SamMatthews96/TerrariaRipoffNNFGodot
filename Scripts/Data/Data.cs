global using CraftingStations = Godot.Collections.Dictionary<
    TerrariaRipoffNNF.StationType,
    TerrariaRipoffNNF.CraftingStation>;
using Godot;

namespace TerrariaRipoffNNF;

public partial class Data : Node {
    public static PackedScenes PackedScenes { get; private set; }
    public static Items Items { get; private set; }
    public static CraftingStations CraftingStations { get; private set; }
    public static Recipes Recipes { get; private set; }

    [Export] private PackedScenes _packedScenes;
    [Export] private Items _items;
    [Export] private CraftingStations _craftingStations;

    public override void _Ready() {
        PackedScenes = _packedScenes;
        Items = _items;
        CraftingStations = _craftingStations;
        Recipes = new Recipes();
    }
}