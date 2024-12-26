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

/*  What should this class be responsible for?
    Crafting items
        When an ingredient is changed

 */

public sealed partial class Crafting : Node {
    [Export] private Player _player;
    [Export] private CraftingStation _handcrafting;

    private Godot.Collections.Dictionary<CraftingStationType, CraftingStation> _availableCraftingStations = new();
    private Recipe _selectedRecipe;

    private Godot.Collections.Dictionary<string, Item> _selectedIngredients = new();


    public event Action<CraftingStation> CraftingStationAdded;
    public event Action<CraftingStation> CraftingStationRemoved;
    public event Action<StackedItems, List<StackedItems>> ItemCrafted;

    public override void _Ready() {
        AddCraftingStation(_handcrafting);
        Manager.Instance.Game.Interface.CraftingInterface.SelectRecipeContainer.RecipeButtonClicked +=
            OnRecipeButtonClicked;
        Manager.Instance.Game.Interface.CraftingInterface.SelectIngredientsContainer.IngredientSelected +=
            OnIngredientSelected;
        Manager.Instance.Game.Interface.CraftingInterface.SelectIngredientsContainer.CraftButtonPressed +=
            OnCraftButtonPressed;
    }

    private void OnCraftButtonPressed() {
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        ItemCrafted?.Invoke(newItems, GetTotalSelectedIngredients());
    }

    private void OnIngredientSelected(string key, Item item) {
        _selectedIngredients[key] = item;
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