using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientsContainer : Container {
    [Export] private Button _recipeNameButton;
    [Export] private Crafting _craftingInterface;
    [Export] private Container _ingredientContainer;
    [Export] private Button _craftButton;
    [Export] private TextureRect _resultItemIcon;

    public event Action<Control, RecipeIngredientSlot> IngredientIconMouseEntered;
    public event Action IngredientIconMouseLeft;
    public event Action<string, Item> IngredientSelected;
    public event Action CraftButtonPressed;

    public override void _Ready() {
        Hide();
        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        _craftButton.ButtonDown += OnCraftButtonDown;
    }

    private void OnCraftButtonDown() {
        CraftButtonPressed?.Invoke();
    }

    public override void _ExitTree() {
        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _recipeNameButton.Text = recipe.Name;
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        foreach (RecipeIngredientSlot ingredientSlot in recipe.IngredientSlots.Values) {
            SelectIngredientMouseover newIngredientMouseover
                = SelectIngredientMouseover.Create(ingredientSlot);
            newIngredientMouseover.MouseEnteredIcon += OnIngredientIconMouseEntered;
            newIngredientMouseover.MouseLeftIcon += OnIngredientIconMouseLeft;
            _ingredientContainer.AddChild(newIngredientMouseover);
        }

        _resultItemIcon.Texture = recipe.ResultIcon;
        Show();
    }

    private void OnIngredientIconMouseEntered(Control node, RecipeIngredientSlot ingredientSlot) {
        IngredientIconMouseEntered?.Invoke(node, ingredientSlot);
    }

    private void OnIngredientIconMouseLeft() {
        IngredientIconMouseLeft?.Invoke();
    }
}