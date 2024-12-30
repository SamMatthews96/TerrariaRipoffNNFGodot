using Godot;

namespace TerrariaRipoffNNF;

public partial class Data : Node {
    public static PackedScenes PackedScenes { get; private set; }
    public static AllRecipes AllRecipes { get; private set; }

    [Export] private PackedScenes _packedScenes;
    [Export] private AllRecipes _allRecipes;

    public override void _Ready() {
        PackedScenes = _packedScenes;
        AllRecipes = _allRecipes;
    }
}