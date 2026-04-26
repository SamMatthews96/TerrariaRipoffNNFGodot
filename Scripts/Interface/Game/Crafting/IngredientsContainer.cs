using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class IngredientsContainer : Container {
    [Export] private Button _recipeNameButton;
    [Export] private Crafting _craftingInterface;
    [Export] private Container _ingredientContainer;
    [Export] private Button _craftButton;
    [Export] private TextureRect _resultItemIcon;
    [Export] private IngredientPopup _ingredientPopup;

    private Dictionary<string, IngredientMouseover> _selectIngredientMouseovers = new();

    private Recipe _selectedRecipe;
    private Player _player;

    public event Action<Control, Ingredient, string> IngredientIconMouseEntered;
    public event Action IngredientIconMouseLeft;
    public event Action CraftButtonPressed;

    public override void _Ready() {
        Hide();
        foreach (Node node in _ingredientContainer.GetChildren()) {
            node.QueueFree();
        }

        _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned +=
            OnLocalPlayerSpawned;
        _craftingInterface.RecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        _ingredientPopup.SelectIngredientButtonClicked += OnSelectIngredientButtonClicked;
        _craftButton.ButtonDown += OnCraftButtonDown;
        TreeExiting += () => {
            _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned -=
                OnLocalPlayerSpawned;
            _craftingInterface.RecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
            _ingredientPopup.SelectIngredientButtonClicked -= OnSelectIngredientButtonClicked;
            _craftButton.ButtonDown -= OnCraftButtonDown;
        };
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

    private void OnSelectIngredientButtonClicked(
        Item selectedIngredient, string slotName
    ) {
        _selectIngredientMouseovers[slotName].Texture = selectedIngredient.IconTexture;
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

        foreach ((string slotName, Ingredient ingredient) in recipe.RecipeIngredients) {
            IngredientMouseover newIngredientMouseover
                = IngredientMouseover.Create(ingredient);
            _selectIngredientMouseovers.Add(slotName, newIngredientMouseover);

            void Entered() {
                IngredientIconMouseEntered?.Invoke(
                    newIngredientMouseover, ingredient, slotName);
            }

            void Exited() {
                IngredientIconMouseLeft?.Invoke();
            }

            newIngredientMouseover.MouseEntered += Entered;
            newIngredientMouseover.MouseExited += Exited;
            TreeExiting += () => {
                newIngredientMouseover.MouseEntered -= Entered;
                newIngredientMouseover.MouseExited -= Exited;
            };

            _ingredientContainer.AddChild(newIngredientMouseover);
        }

        _resultItemIcon.Texture = recipe.TemplateIcon;
        Show();
    }
}