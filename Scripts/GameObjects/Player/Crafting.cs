using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public enum CraftingStationType {
    Handcrafting,
    Workbench,
    Furnace,
    Anvil,
    AlchemyTable,
    Loom,
    CookingPot,
}


public sealed partial class Crafting : Node {
    [Export] private Player _player;
    [Export] private CraftingStation _handcrafting;

    private Godot.Collections.Dictionary<CraftingStationType, CraftingStation> _availableCraftingStations = new();
    private Recipe _selectedRecipe;

    private Godot.Collections.Dictionary<string, Item> _selectedIngredients = new();
    private Game _game;

    public event Action<CraftingStation> CraftingStationAdded;
    public event Action<CraftingStation> CraftingStationRemoved;
    // public event Action<Godot.Collections.Dictionary<> SelectedIngredientsChanged;
    public event Action<StackedItems, List<StackedItems>> ItemCrafted;

    public void InitAsLocal(Game game) {
        _game = game;
        AddCraftingStation(_handcrafting);
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        craftingInterface.SelectIngredientsContainer.IngredientButtonClicked += OnIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed += OnCraftButtonPressed;
        TreeExiting += OnTreeExitingLocal;
    }

    private void OnTreeExitingLocal() {
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
        craftingInterface.SelectIngredientsContainer.IngredientButtonClicked -= OnIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;
        TreeExiting -= OnTreeExitingLocal;
    }

    private void OnCraftButtonPressed() {
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        if (newItems is null) return;
        ItemCrafted?.Invoke(newItems, GetTotalSelectedIngredients());
    }

    private void OnIngredientButtonClicked(Item item, RecipeIngredientSlot ingredientSlot) {
        _selectedIngredients[ingredientSlot.RecipeSlot] = item;
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

    public override void _ExitTree() { }


    private void AddCraftingStation(CraftingStation craftingStation) {
        _availableCraftingStations[craftingStation.Type] = craftingStation;
        CraftingStationAdded?.Invoke(craftingStation);
    }
}