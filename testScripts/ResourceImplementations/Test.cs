using Godot;
using Godot.Collections;
using TerrariaRipoffNNF;

public partial class Test : Node {
    [Export] private Recipe _recipe;
    [Export] private Dictionary<string, Item> _suppliedIngredients;
    public override void _Ready() {
        StackedItems items = _recipe.Build(_suppliedIngredients);
        GD.Print(items.Item.Name);
    }
}