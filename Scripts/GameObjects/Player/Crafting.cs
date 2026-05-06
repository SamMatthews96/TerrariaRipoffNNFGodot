using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public sealed partial class Crafting : Node {
    [Export] private Area2D _craftingArea;
    [Export] private Player _player;
    private Recipe _selectedRecipe;
    private Dictionary<Vector2I, StationType> _nearbyStations = new();

    private Dictionary<string, Item> _selectedIngredients = new();
    public event Action<StackedItems> SelectedIngredientsChanged;
    public event CraftEventHandler HostItemCrafted;
    public event Action<StationType> AddedNewStation;
    public event Action<StationType> RemovedStation;

    public delegate void CraftEventHandler(
        StackedItems result, Array<StackedItems> ingredients);

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
            _player.World.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
            TreeExiting += () => {
                craftingUi.RecipeContainer.RecipeButtonClicked -= OnRecipeButtonClicked;
                craftingUi.IngredientPopup.SelectIngredientButtonClicked -= OnIngredientButtonClicked;
                craftingUi.IngredientsContainer.CraftButtonPressed -= OnCraftButtonPressed;
                _player.World.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
            };
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        Vector2I playerCoords = _player.SpawnCoords;

        for (int x = playerCoords.X - CraftRange; x <= playerCoords.X + CraftRange; x++) {
            for (int y = playerCoords.Y - CraftRange; y <= playerCoords.Y + CraftRange; y++) {
                Vector2I coords = new(x, y);

                if (!_player.World.StationManager.Stations
                        .TryGetValue(coords, out StationType type)) continue;
                HostAddCraftingStation(coords, type);
            }
        }
    }

    private void OnMovedCellHost(Vector2I newCoords, Vector2I oldCoords) {
        foreach (Vector2I coords in _nearbyStations.Keys) {
            if (_player.World.IsInOrthogonalRange(
                    coords, newCoords, CraftRange)) continue;
            StationType type = _nearbyStations[coords];
            HostRemoveCraftingStation(coords, type);
        }

        // Add new stations that are now in range
        Array<Vector2I> newCells =
            _player.World.GetNewCellsInRange(newCoords, oldCoords, CraftRange);
        foreach (Vector2I coords in newCells) {
            if (!_player.World.StationManager.Stations
                    .TryGetValue(coords, out StationType type)) continue;
            HostAddCraftingStation(coords, type);
        }
    }

    public void HostAddCraftingStation(Vector2I coords, StationType type) {
        if (!_nearbyStations.Values.Contains(type)) {
            RpcId(_player.PeerId, nameof(RpcLocalAddCraftingStation),
                (int)type);
        }
        _nearbyStations[coords] = type;
    }

    [Rpc(CallLocal = true)]
    private void RpcLocalAddCraftingStation(StationType type) {
        AddedNewStation?.Invoke(type);
    }

    public void HostRemoveCraftingStation(Vector2I coords, StationType type) {
        _nearbyStations.Remove(coords);
        if (!_nearbyStations.Values.Contains(type)) {
            RpcId(_player.PeerId,
                nameof(RpcLocalRemoveCraftingStation),
                (int)type);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcLocalRemoveCraftingStation(StationType type) {
        RemovedStation?.Invoke(type);
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