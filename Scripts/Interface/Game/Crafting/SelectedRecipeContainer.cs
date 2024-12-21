using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectedRecipeContainer : Container {
    private Recipe _selectedRecipe;
    [Export] private Button _recipeNameButton;
    [Export] private Crafting _craftingInterface;
    [Export] private Container _ingredientContainer;
    [Export] private Button _craftButton;
    [Export] private TextureRect _resultItemIcon;
    [Export] private Control _ingredientPopupPanel;

    public event Action<Control, IngredientType> IngredientIconMouseEntered;
    public event Action IngredientIconMouseLeft;
    
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
            newIngredientSlotTexture.MouseLeftIcon += OnIngredientIconMouseLeft;
            _ingredientContainer.AddChild(newIngredientSlotTexture);
        }

        _resultItemIcon.Texture = recipe.ResultIcon;
        Show();
    }

    private void OnIngredientIconMouseEntered(Control node, IngredientType ingredientType) {
        IngredientIconMouseEntered?.Invoke(node, ingredientType);
    }
    
    private void OnIngredientIconMouseLeft() {
        IngredientIconMouseLeft?.Invoke();
    }
}