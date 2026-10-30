using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF;

public delegate void CraftEventDelegate(
    StackedItems result, Array<StackedItems> ingredients);

public sealed partial class Crafting : Node {
    [Export] private Area2D _craftingArea;
    [Export] private Player _player;
    private StationManager _stationManager;
    private World _world;
    
    private Recipe _selectedRecipe;
    private Dictionary<Vector2I, StationType> _nearbyStations = new();

    private Dictionary<string, Item> _selectedIngredients = new();
    public event Action<StackedItems> SelectedIngredientsChanged;
    public event CraftEventDelegate HostItemCrafted;
    public event Action<StationType> AddedNewStation;
    public event Action<StationType> RemovedStation;

    public int CraftRange { get; private set; } = 8;
    
    // monitors the crafting state for one character

    public override void _Ready() {
        
        _world = _player.World;
        _stationManager = _player.World.StationManager;
        if (_player.World.IsHost) {
            _player.CellMoved += OnHostCellMoved;
            _stationManager.StationCreated += OnStationCreated;
            _stationManager.StationDestroyed += OnStationDestroyed;
            
            TreeExiting += () => {
                _player.CellMoved -= OnHostCellMoved;
                _stationManager.StationCreated -= OnStationCreated;
                _stationManager.StationDestroyed -= OnStationDestroyed;
            };
        }

        if (_player.IsLocalPlayer) {
            Interface.Crafting craftingUi = _player.World.CraftingInterface;
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
    
    private void OnStationCreated(Vector2I coords, StationType type) {
        if (_player.World.IsInOrthogonalRange(
                _player.Coords, coords, CraftRange)) {
            HostAddCraftingStation(coords, type);
        }
    }
    
    private void OnStationDestroyed(Vector2I coords, StationType type) {
        if (_player.World.IsInOrthogonalRange(
                _player.Coords, coords, CraftRange)) {
            HostRemoveCraftingStation(coords, type);
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        Array<Vector2I> localCells = _world.GetCellsInRange(player.Coords, CraftRange);
        Dictionary<Vector2I, StationType> localStations = 
            _stationManager.GetStationsInRegion(localCells);
        foreach ((Vector2I coords, StationType type) in localStations) {
            HostAddCraftingStation(coords, type);
        }
    }

    private void OnHostCellMoved(Vector2I newCoords, Vector2I oldCoords) {
        foreach (Vector2I coords in _nearbyStations.Keys) {
            if (_world.IsInOrthogonalRange(
                    coords, newCoords, CraftRange)) continue;
            StationType type = _nearbyStations[coords];
            HostRemoveCraftingStation(coords, type);
        }

        Array<Vector2I> newCells =
            _player.World.GetNewCellsInRange(newCoords, oldCoords, CraftRange);
        Dictionary<Vector2I, StationType> localStations = 
            _stationManager.GetStationsInRegion(newCells);
        foreach ((Vector2I coords, StationType type) in localStations) {
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

    private void HostRemoveCraftingStation(Vector2I coords, StationType type) {
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
        
        this.RpcHost(nameof(RpcHostTryCraft),
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