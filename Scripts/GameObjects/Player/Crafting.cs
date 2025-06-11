using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TerrariaRipoffNNF;

public sealed partial class Crafting : Node {
    [Export] private Player _player;
    private Recipe _selectedRecipe;

    private Godot.Collections.Dictionary<string, Item> _selectedIngredients = new();
    private Game _game;

    public event Action<CraftingStation> CraftingStationAdded;
    public event Action<CraftingStation> CraftingStationRemoved;
    public event Action<StackedItems> SelectedIngredientsChanged;
    public event Action<StackedItems, List<StackedItems>> ItemCrafted;

    public void InitAsLocal(Game game) {
        _game = game;
        AddCraftingStation(CraftingStationType.Handcrafting);
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        craftingInterface.SelectIngredientPopup.SelectIngredientButtonClicked += OnSelectIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed += OnCraftButtonPressed;
        _game.WorldObjectManager.CraftStationPlaced += OnCraftStationPlaced;
        TreeExiting += OnTreeExitingLocal;
    }

    private void OnCraftStationPlaced(Item item, IntVector coords) {
        int distance = IntVector.GetOrthogonalDistance(_player.Coords, coords);
        if (distance > WorldObjectManager.BlockSpawnDistance) return;
        AddCraftingStation(item.GetProperty<ItemCraftStation>().Type);
    }

    private void OnTreeExitingLocal() {
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
        craftingInterface.SelectIngredientPopup.SelectIngredientButtonClicked -= OnSelectIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;
        TreeExiting -= OnTreeExitingLocal;
    }

    private void OnCraftButtonPressed() {
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        if (newItems is null) return;
        List<StackedItems> totalIngredients = GetTotalSelectedIngredients();
        if (totalIngredients.Any(
                stackedItems => !_player.Inventory.IsContainingStackedItems(stackedItems))) return;

        ItemCrafted?.Invoke(newItems, GetTotalSelectedIngredients());
    }

    private void OnSelectIngredientButtonClicked(Item item, RecipeIngredientSlot ingredientSlot) {
        _selectedIngredients[ingredientSlot.RecipeSlot] = item;
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        SelectedIngredientsChanged?.Invoke(newItems);
    }

    private List<StackedItems> GetTotalSelectedIngredients() {
        List<StackedItems> totalIngredients = new();
        foreach (string key in _selectedRecipe.IngredientSlots.Keys) {
            if (!_selectedIngredients.TryGetValue(key, out Item item)) continue;
            int amount = _selectedRecipe.IngredientSlots[key].Amount;
            totalIngredients.Add(new StackedItems(item, amount));
        }

        return totalIngredients;
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _selectedRecipe = recipe;
        _selectedIngredients.Clear();
    }


    private void AddCraftingStation(CraftingStationType type) {
        CraftingStationAdded?.Invoke(Data.CraftingStations[type]);
    }

    private void RemoveCraftingStation(CraftingStation craftingStation) {
        CraftingStationRemoved?.Invoke(craftingStation);
    }
}