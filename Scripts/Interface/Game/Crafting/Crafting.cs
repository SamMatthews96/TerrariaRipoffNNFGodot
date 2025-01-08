using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Crafting : Control {
    [Export] private Game _gameInterface;
    [Export] private Button _selectCraftingStationMenuButton;

    [Export] public SelectStationContainer CraftSelectStationContainer { get; private set; }
    [Export] public SelectRecipeContainer SelectRecipeContainer { get; private set; }
    [Export] public SelectIngredientsContainer SelectIngredientsContainer { get; private set; }
    [Export] public SelectIngredientPopup SelectIngredientPopup { get; private set; }

    public override void _Ready() {
        Hide();
        _gameInterface.GameManager.InputManager.CraftMenuPressed += OnCraftMenuPressed;
    }

    private void OnCraftMenuPressed() {
        if (Visible) {
            Hide();
        } else {
            Show();
        }
    }
}