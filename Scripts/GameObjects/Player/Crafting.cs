using System;
using System.Linq;
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
    // public event Action<StackedItems, List<StackedItems>> ItemCrafted;

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

    private void OnCraftButtonPressed() {
        if (!_player.World.IsHost) {
            if (!IsCraftValid()) return;
        }

        RpcId(1, nameof(RpcHostTryCraft));

        
    }

    private bool IsCraftValid() {
        // KeyValuePair<string, RecipeIngredientSlot> slot
        foreach ((string key, Ingredient slot) in _selectedRecipe.RecipeIngredients) {
            if (slot.Required && !_selectedIngredients.ContainsKey(key)) {
                return false;
            }
        }

        foreach (string key in _selectedRecipe.RecipeIngredients.Keys) {
            if (!_selectedIngredients.TryGetValue(key, out Item item)) continue;
            int amount = _selectedRecipe.RecipeIngredients[key].Amount;
            StackedItems stackedItems = new(item, amount);
            if (!_player.Inventory.IsContainingStackedItems(stackedItems)) {
                return false;
            }
        }

        return true;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostTryCraft(Recipe recipe, Dictionary<string, Item> ingredients) {
        if (!IsCraftValid()) return;
        
        // craft on host
        
        int senderId = Multiplayer.GetRemoteSenderId();
        if (senderId == 1) return;
        // craft on client
        
    }

    private void CraftRecipe(Recipe recipe, Dictionary<string, Item> ingredients) {
        StackedItems newItems = recipe.Build(ingredients);
        var temp = ingredients.Values.ToList();
        foreach (Item item in temp) {
            
        }
    }

    private void OnIngredientButtonClicked(Item item, string slotName) {
        _selectedIngredients[slotName] = item;
        StackedItems newItems = _selectedRecipe.Build(_selectedIngredients);
        SelectedIngredientsChanged?.Invoke(newItems);
    }

    private void OnRecipeButtonClicked(Recipe recipe) {
        _selectedRecipe = recipe;
        _selectedIngredients.Clear();
    }
}