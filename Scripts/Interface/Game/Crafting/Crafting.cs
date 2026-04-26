using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Crafting : Control {
    [Export] public Game GameInterface { get; private set; }
    [Export] private Button _selectCraftingStationMenuButton;

    [Export] public StationContainer StationContainer { get; private set; }
    [Export] public RecipeContainer RecipeContainer { get; private set; }
    [Export] public IngredientsContainer IngredientsContainer { get; private set; }
    [Export] public IngredientPopup IngredientPopup { get; private set; }

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