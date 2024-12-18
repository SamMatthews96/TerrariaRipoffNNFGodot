using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectedRecipeContainer : Container {
    private Recipe _selectedRecipe;
    [Export] private Button _recipeNameButton;
    [Export] private Main _craftingInterface;
    [Export] private Container _ingredientContainer;
    [Export] private Button _craftButton;
    [Export] private TextureRect _resultItemIcon;
    [Export] private Control _ingredientPopupPanel;
    


    public override void _Ready() {
        Hide();
        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }
    }

    public override void _ExitTree() {
        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _recipeNameButton.Text = recipe.Name;
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        foreach ((string itemAttributeSlot, RecipeIngredientSlot ingredientSlot)
                 in recipe.Ingredients) {
            RecipeIngredientSlotTexture newIngredientSlotTexture
                = RecipeIngredientSlotTexture.Create(ingredientSlot);
            newIngredientSlotTexture.MouseEnteredIcon += OnIngredientIconMouseEntered;
            _ingredientContainer.AddChild(newIngredientSlotTexture);
        }
        
        _resultItemIcon.Texture = recipe.ResultIcon;
        Show();
    }

    private void OnIngredientIconMouseEntered(Vector2 iconPosition, RecipeIngredientSlot ingredientSlot) {
        _ingredientPopupPanel.GlobalPosition = iconPosition;
        _ingredientPopupPanel.Show();
        //@todo move this logic into the popup panel
    }
}