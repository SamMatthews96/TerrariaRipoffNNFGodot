using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TerrariaRipoffNNF;

public sealed partial class Crafting : Node {
    [Export] private Area2D _craftingArea;
    private Player _player;
    private Recipe _selectedRecipe;

    private Godot.Collections.Dictionary<string, Item> _selectedIngredients = new();
    private Game _game;
    public List<CraftStationArea> LocalCraftStationsAreas = new();
    public event Action<CraftingStationType> CraftingStationAdded;
    public event Action<CraftingStationType> CraftingStationRemoved;
    public event Action<StackedItems> SelectedIngredientsChanged;
    public event Action<StackedItems, List<StackedItems>> ItemCrafted;

    public static Crafting Create(Game game, Player player) {
        Crafting crafting = Data.PackedScenes.PlayerCrafting
            .Instantiate<Crafting>();
        crafting._game = game;
        crafting._player = player;
        return crafting;
    }

    public override void _Ready() {
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        craftingInterface.SelectIngredientPopup.SelectIngredientButtonClicked += OnSelectIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed += OnCraftButtonPressed;

        _craftingArea.AreaEntered += OnCraftingAreaEntered;
        _craftingArea.AreaExited += OnCraftingAreaExited;

        CraftingStationAdded?.Invoke(CraftingStationType.Handcrafting);
    }

    public override void _ExitTree() {
        Interface.Crafting craftingInterface = _game.Interface.CraftingInterface;
        craftingInterface.SelectRecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
        craftingInterface.SelectIngredientPopup.SelectIngredientButtonClicked -= OnSelectIngredientButtonClicked;
        craftingInterface.SelectIngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;

        _craftingArea.AreaEntered -= OnCraftingAreaEntered;
        _craftingArea.AreaExited -= OnCraftingAreaExited;
    }

    private void OnCraftingAreaEntered(Area2D area) {
        if (area is not CraftStationArea craftStationArea) {
            throw new Exception("[20250617.1422.1] Crafting area entered by non-crafting area");
        }

        // CraftingStationType newType = craftStationArea.CraftStation.Type;
        // if (!LocalCraftStationsAreas.Exists(
        //         currentArea => currentArea.CraftStation.Type == newType)) {
        //     CraftingStationAdded?.Invoke(newType);
        // }

        LocalCraftStationsAreas.Add(craftStationArea);
    }

    private void OnCraftingAreaExited(Area2D area) {
        if (area is not CraftStationArea craftStationArea) {
            throw new Exception("[20250617.1424.1] Crafting area entered by non-crafting area");
        }

        // CraftingStationType exitingType = craftStationArea.CraftStation.Type;
        // LocalCraftStationsAreas.Remove(craftStationArea);
        // if (!LocalCraftStationsAreas.Exists(
        //         currentArea => currentArea.CraftStation.Type == exitingType)) {
        //     CraftingStationRemoved?.Invoke(exitingType);
        // }
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
        foreach (string key in _selectedRecipe.RecipeIngredients.Keys) {
            if (!_selectedIngredients.TryGetValue(key, out Item item)) continue;
            int amount = _selectedRecipe.RecipeIngredients[key].Amount;
            totalIngredients.Add(new StackedItems(item, amount));
        }

        return totalIngredients;
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _selectedRecipe = recipe;
        _selectedIngredients.Clear();
    }
}