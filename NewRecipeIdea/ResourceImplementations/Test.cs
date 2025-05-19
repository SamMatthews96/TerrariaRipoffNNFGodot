using Godot;
using System;
using Godot.Collections;
using TerrariaRipoffNNF;

public partial class Test : Node {
    [Export] DynamicRecipe _dynamicRecipe;
    [Export] Dictionary<string, Item> _suppliedIngredients;
    public override void _Ready() {
        Item item = _dynamicRecipe.BuildFromTemplate(_suppliedIngredients);
        GD.Print(item.InventorySpace);
        GD.Print(item.Name);
    }
}