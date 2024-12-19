using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientPopup : Control {
    [Export] private Container _iconContainer;
    [Export] private SelectedRecipeContainer _selectedRecipeContainer;
    public override void _Ready() {
        Hide();
        foreach (Node child in _iconContainer.GetChildren()) {
            child.QueueFree();
        }
        
        _selectedRecipeContainer.IngredientIconMouseEntered += OnIngredientIconMouseEntered;
        _selectedRecipeContainer.IngredientIconMouseLeft += OnIngredientIconMouseLeft;
    }

    private void OnIngredientIconMouseEntered(Control node, IngredientType ingredientType) {
        Position = node.GlobalPosition;
        Show();
    }

    private void OnIngredientIconMouseLeft() {
        Hide();
    }
}