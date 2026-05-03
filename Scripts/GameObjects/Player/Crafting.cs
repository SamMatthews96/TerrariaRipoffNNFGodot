using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public sealed partial class Crafting : Node {
    [Export] private Area2D _craftingArea;
    [Export] private Player _player;
    private Recipe _selectedRecipe;
    private Dictionary<Prop, CraftingStationType> _nearbyCraftingStations = new();
    private Array<CraftingStationType> _nearbyStationTypes = new();

    private Dictionary<string, Item> _selectedIngredients = new();
    public event Action<StackedItems> SelectedIngredientsChanged;

    public delegate void CraftEventHandler(
        StackedItems result, Array<StackedItems> ingredients);

    public event CraftEventHandler HostItemCrafted;
    public int CraftRange { get; private set; } = 8;

    public override void _Ready() {
        if (_player.World.IsHost) {
            _player.MovedCellHost += OnMovedCellHost;
            TreeExiting += () => { _player.MovedCellHost -= OnMovedCellHost; };
        }

        if (_player.IsLocalPlayer) {
            Interface.Crafting craftingUi = _player.World.Interface.CraftingInterface;
            craftingUi.RecipeContainer.RecipeButtonClicked += OnRecipeButtonClicked;
            craftingUi.IngredientPopup.SelectIngredientButtonClicked += OnIngredientButtonClicked;
            craftingUi.IngredientsContainer.CraftButtonPressed += OnCraftButtonPressed;
            TreeExiting += () => {
                craftingUi.RecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
                craftingUi.IngredientPopup.SelectIngredientButtonClicked -= OnIngredientButtonClicked;
                craftingUi.IngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;
            };
        }
    }

    private void OnMovedCellHost(Vector2I newCoords, Vector2I oldCoords) {
        // Remove stations that are no longer in range
        foreach (Prop prop in _nearbyCraftingStations.Keys) {
            bool inRange = false;
            foreach (Vector2I propCell in prop.Cells) {
                if (_player.World.IsInOrthogonalRange(newCoords, propCell, CraftRange)) {
                    inRange = true;
                    break;
                }
            }

            if (!inRange) {
                HostRemoveCraftingStation(prop);
            }
        }

        // Add new stations that are now in range
        Array<Vector2I> newCells =
            _player.World.GetNewCellsInRange(newCoords, oldCoords, CraftRange);
        foreach (Vector2I coords in newCells) {
            if (!_player.World.PropManager.PropCells
                    .TryGetValue(coords, out Prop prop)) continue;
            if (!prop.Item.GetProperty<ItemProp>().HasProperty<PropStation>()) continue;
            if (_nearbyCraftingStations.ContainsKey(prop)) continue;
            HostAddCraftingStation(prop);
        }
    }

    public void HostAddCraftingStation(Prop prop) {
        PropStation station =
            prop.Item.GetProperty<ItemProp>().GetProperty<PropStation>();
        _nearbyCraftingStations[prop] = station.Type;
        if (!_nearbyStationTypes.Contains(station.Type)) {
            RpcId(_player.PeerId, 
                nameof(RpcLocalAddCraftingStation), 
                (int)station.Type);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcLocalAddCraftingStation(CraftingStationType type) {
        _nearbyStationTypes.Add(type);
    }

    public void HostRemoveCraftingStation(Prop prop) {
        CraftingStationType type = _nearbyCraftingStations[prop];
        _nearbyCraftingStations.Remove(prop);
        if (!_nearbyCraftingStations.Values.Contains(type)) {
            
            RpcId(_player.PeerId, 
                nameof(RpcLocalRemoveCraftingStation), 
                (int)type);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcLocalRemoveCraftingStation(CraftingStationType type) {
        _nearbyStationTypes.Remove(type);
    }

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