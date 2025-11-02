using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectIngredientsContainer : Container {
    [Export] private Button _recipeNameButton;
    [Export] private Crafting _craftingInterface;
    [Export] private Container _ingredientContainer;
    [Export] private Button _craftButton;
    [Export] private TextureRect _resultItemIcon;
    [Export] private SelectIngredientPopup _ingredientPopup;

    private Dictionary<string, SelectIngredientMouseover> _selectIngredientMouseovers = new();

    private Recipe _selectedRecipe;
    private Player _player;

    public event Action<Control, RecipeIngredientSlot> IngredientIconMouseEntered;
    public event Action IngredientIconMouseLeft;
    public event Action CraftButtonPressed;

    public override void _Ready() {
        Hide();
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        _ingredientPopup.SelectIngredientButtonClicked += OnSelectIngredientButtonClicked;
        _craftButton.ButtonDown += OnCraftButtonDown;
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        _craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
        _ingredientPopup.SelectIngredientButtonClicked -= OnSelectIngredientButtonClicked;
        _craftButton.ButtonDown -= OnCraftButtonDown;
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        _player = player;
        _player.Crafting.SelectedIngredientsChanged += OnSelectedIngredientsChanged;
    }

    private void OnSelectedIngredientsChanged(StackedItems newItems) {
        _resultItemIcon.Texture = newItems is null
            ? _selectedRecipe.TemplateIcon
            : newItems.Item.IconTexture;
    }

    private void OnSelectIngredientButtonClicked(Item selectedIngredient, RecipeIngredientSlot ingredientSlot) {
        _selectIngredientMouseovers[ingredientSlot.RecipeSlot].Texture = selectedIngredient.IconTexture;
    }


    private void OnCraftButtonDown() {
        CraftButtonPressed?.Invoke();
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _selectedRecipe = recipe;
        _recipeNameButton.Text = recipe.RecipeName;
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        _selectIngredientMouseovers.Clear();

        foreach ((string slotName, RecipeIngredientSlot ingredientSlot) in recipe.RecipeIngredients) {
            SelectIngredientMouseover newIngredientMouseover
                = SelectIngredientMouseover.Create(ingredientSlot);
            _selectIngredientMouseovers.Add(slotName, newIngredientMouseover);
            newIngredientMouseover.MouseEnteredIcon += OnIngredientIconMouseEntered;
            newIngredientMouseover.MouseLeftIcon += OnIngredientIconMouseLeft;
            _ingredientContainer.AddChild(newIngredientMouseover);
        }

        _resultItemIcon.Texture = recipe.TemplateIcon;
        Show();
    }

    private void OnIngredientIconMouseEntered(Control node, RecipeIngredientSlot ingredientSlot) {
        IngredientIconMouseEntered?.Invoke(node, ingredientSlot);
    }

    private void OnIngredientIconMouseLeft() {
        IngredientIconMouseLeft?.Invoke();
    }
}