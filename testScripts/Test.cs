using Godot;

namespace TerrariaRipoffNNF;

public partial class Test : Node {
    [Export] private AllRecipes _allRecipes;
    
    private enum TestEnum {
        Test1,
        Test2,
        Test3
    }
    public override void _Ready() {
        CraftingStationRecipes temp = (_allRecipes.Recipes[CraftingStationType.Handcrafting]);
        GD.Print(temp.Recipes[0].ResourcePath);
    } 
}