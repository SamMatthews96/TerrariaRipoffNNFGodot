using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public sealed partial class Crafting : Node {
    [Export] private Area2D _craftingArea;
    [Export] private Player _player;
    private Recipe _selectedRecipe;

    private Dictionary<string, Item> _selectedIngredients = new();
    public event Action<CraftingStationType> CraftingStationAdded;
    public event Action<CraftingStationType> CraftingStationRemoved;
    public event Action<StackedItems> SelectedIngredientsChanged;
    public delegate void CraftEventHandler(StackedItems result, Array<StackedItems> ingredients);
    public event CraftEventHandler HostItemCrafted;

    public override void _Ready() {
        if (!_player.IsLocalPlayer) return;
        Interface.Crafting craftingInterface = _player.World.Interface.CraftingInterface;

        craftingInterface.RecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
        craftingInterface.IngredientPopup.SelectIngredientButtonClicked += OnIngredientButtonClicked;
        craftingInterface.IngredientsContainer.CraftButtonPressed += OnCraftButtonPressed;

        TreeExiting += () => {
            craftingInterface.RecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
            craftingInterface.IngredientPopup.SelectIngredientButtonClicked -= OnIngredientButtonClicked;
            craftingInterface.IngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;
        };
    }

    // private void OnCraftingAreaEntered(Area2D area) {
    //     if (area is not CraftStationArea craftStationArea) {
    //         throw new Exception("[20250617.1422.1] Crafting area entered by non-crafting area");
    //     }
    //
    //     CraftingStationType newType = craftStationArea.CraftStation.Type;
    //     if (!LocalCraftStationsAreas.Exists(
    //             currentArea => currentArea.CraftStation.Type == newType)) {
    //         CraftingStationAdded?.Invoke(newType);
    //     }
    //
    //     LocalCraftStationsAreas.Add(craftStationArea);
    // }

    // private void OnCraftingAreaExited(Area2D area) {
    //     if (area is not CraftStationArea craftStationArea) {
    //         throw new Exception("[20250617.1424.1] Crafting area entered by non-crafting area");
    //     }
    //
    //     CraftingStationType exitingType = craftStationArea.CraftStation.Type;
    //     LocalCraftStationsAreas.Remove(craftStationArea);
    //     if (!LocalCraftStationsAreas.Exists(
    //             currentArea => currentArea.CraftStation.Type == exitingType)) {
    //         CraftingStationRemoved?.Invoke(exitingType);
    //     }
    // }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _selectedRecipe = recipe;
        _selectedIngredients.Clear();
    }

    private void OnIngredientButtonClicked(Item item, string slotName) {
        _selectedIngredients[slotName] = item;
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        SelectedIngredientsChanged?.Invoke(newItems);
    }

    private void OnCraftButtonPressed() {
        if (!_player.World.IsHost) {
            if (!IsCraftValid(_selectedRecipe, _selectedIngredients)) return;
        }

        Dictionary<string, Dictionary> ingredientsDictionary = new();
        foreach ((string key, Item item) in _selectedIngredients) {
            Dictionary itemDictionary = item.ToDictionary();
            ingredientsDictionary[key] = itemDictionary;
        }

        RpcId(1, nameof(RpcHostTryCraft),
            _selectedRecipe.Id, ingredientsDictionary);
    }
    
    

    private bool IsCraftValid(
        Recipe recipe, Dictionary<string, Item> ingredients
    ) {
        foreach ((string key, Ingredient slot) in recipe.RecipeIngredients) {
            if (slot.Required && !ingredients.ContainsKey(key)) {
                return false;
            }
        }

        foreach (string key in recipe.RecipeIngredients.Keys) {
            if (!ingredients.TryGetValue(key, out Item item)) continue;
            int amount = recipe.RecipeIngredients[key].Amount;
            StackedItems stackedItems = new(item, amount);
            if (!_player.Inventory.IsContainingStackedItems(stackedItems)) {
                return false;
            }
        }

        return true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostTryCraft(
        int recipeId, Dictionary<string, Dictionary> ingredientsDictionary
    ) {
        Recipe recipe = Data.Recipes.GetRecipe(recipeId);
        Dictionary<string, Item> ingredients = new();
        foreach ((string key, Dictionary ingredientDict) in ingredientsDictionary) {
            ingredients[key] = Item.FromDictionary(ingredientDict);
        }

        if (!IsCraftValid(recipe, ingredients)) return;

        StackedItems newItems = recipe.Build(ingredients);

        Array<StackedItems> ingredientsArray = new();
        foreach ((string slotName, Item item) in ingredients) {
            int amount = recipe.RecipeIngredients[slotName].Amount;
            StackedItems stackedItems = new(item, amount);
            ingredientsArray.Add(stackedItems);
        }
    
        HostItemCrafted?.Invoke(newItems, ingredientsArray);
    }
}