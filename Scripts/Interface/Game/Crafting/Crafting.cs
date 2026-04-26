using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Crafting : Control {
    [Export] public Game GameInterface { get; private set; }
    [Export] private Button _selectCraftingStationMenuButton;

    [Export] public SelectStationContainer CraftSelectStationContainer { get; private set; }
    [Export] public SelectRecipeContainer SelectRecipeContainer { get; private set; }
    [Export] public SelectIngredientsContainer SelectIngredientsContainer { get; private set; }
    [Export] public SelectIngredientPopup SelectIngredientPopup { get; private set; }

    public override void _Ready() {
        Hide();
        GameInterface.World.InputManager.ToggleInventoryPressed += OnToggleInventoryPressed;
        GameInterface.World.InputManager.EscapePressed += OnEscapePressed;
        
    }

    private void OnToggleInventoryPressed() {
        if (Visible) {
            Hide();
        } else {
            Show();
        }
    }

    private void OnEscapePressed() {
        if (Visible) {
            Hide();
        }
    }
}