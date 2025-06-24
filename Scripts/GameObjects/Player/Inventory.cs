using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 50;
    public float UsedSpace { get; private set; }

    public List<StackedItems> StackedItemsList => _inventoryItemsList;

    public bool IsContainingStackedItems(StackedItems stackedItems) {
        foreach (StackedItems inventoryStackedItems in StackedItemsList) {
            if (inventoryStackedItems.Item == stackedItems.Item) {
                return inventoryStackedItems.Count >= stackedItems.Count;
            }
        }

        return false;
    }

    public event Action<StackedItems> ItemStackChangedSize;
    public event Action<StackedItems> AddedItemStack;
    public event Action<StackedItems> RemovedItemStack;
    public event Action<WorldObject> PickupLooted;

    public event Action<Item> EquipItemClicked;

    private Player _player;
    private Game _game;
    private Dictionary _playerData;
    private readonly List<StackedItems> _inventoryItemsList = new();

    public static Inventory Create(
        Game game, Dictionary playerData, Player player
    ) {
        Inventory inventory = new();
        inventory._game = game;
        inventory._player = player;
        inventory._playerData = playerData;

        return inventory;
    }

    public override void _Ready() {
        _player.Crafting.ItemCrafted += OnItemCrafted;
        _player.PickupArea.TouchedItem += OnCollidedWithPickup;
        _game.Interface.InventoryUi.ItemActionClicked += OnItemActionClicked;
        
        if (!_playerData.TryGetValue("Inventory", out Variant inventoryData)) return;
        if (!inventoryData.AsGodotDictionary<string, Array>().TryGetValue(
                "InventoryItemsList", out Array inventoryItems)) return;

        foreach (Dictionary savedItem in inventoryItems) {
            Item newItem = Item.FromDictionary(savedItem["Item"].AsGodotDictionary());
            int count = (int)savedItem["Count"].ToString().ToFloat();
            StackedItems newStack = new(newItem, count);
            AddItems(newStack);
        }
    }

    public override void _ExitTree() {
        _game.Interface.InventoryUi.ItemActionClicked -= OnItemActionClicked;
        _player.Crafting.ItemCrafted -= OnItemCrafted;
    }

    private void OnItemActionClicked(StackedItems stackedItems) {
        if (stackedItems.Item.HasProperty<ItemEquipment>()) {
            EquipItemClicked?.Invoke(stackedItems.Item);
        }
    }

    private void OnItemCrafted(StackedItems newItems, List<StackedItems> ingredients) {
        AddItems(newItems);
        foreach (StackedItems ingredient in ingredients) {
            RemoveItems(ingredient);
        }
    }

    private void OnCollidedWithPickup(WorldPickup pickup) {
        if (pickup.Item.InventorySpace > MaximumSpace - UsedSpace) {
            return;
        }

        StackedItems items = new(pickup.Item);

        AddItems(items);
        PickupLooted?.Invoke(pickup.WorldObject);
    }

    public void OnAfterBuildSuccess(Item item) {
        StackedItems inventoryItems = new(item, 1);
        RemoveItems(inventoryItems);
    }

    private void AddItems(StackedItems inventoryItemsToAdd) {
        UsedSpace += inventoryItemsToAdd.TotalSpace;

        int index = _inventoryItemsList.FindIndex(inventoryItems =>
            Item.AreEqual(inventoryItems.Item, inventoryItemsToAdd.Item));

        if (index == -1) {
            _inventoryItemsList.Add(inventoryItemsToAdd);
            AddedItemStack?.Invoke(inventoryItemsToAdd);
        } else {
            _inventoryItemsList[index] += inventoryItemsToAdd;
            ItemStackChangedSize?.Invoke(_inventoryItemsList[index]);
        }
    }

    private void RemoveItems(StackedItems inventoryItemsToRemove) {
        UsedSpace -= inventoryItemsToRemove.TotalSpace;

        int index = _inventoryItemsList.FindIndex(inventoryItems =>
            inventoryItems.Item == inventoryItemsToRemove.Item);

        if (index == -1) {
            throw new Exception("[20240815.0934.1] Inventory item not found");
        }

        _inventoryItemsList[index] -= inventoryItemsToRemove;

        switch (_inventoryItemsList[index].Count) {
            case > 0:
                ItemStackChangedSize?.Invoke(_inventoryItemsList[index]);
                break;
            case 0:
                _inventoryItemsList.RemoveAt(index);
                RemovedItemStack?.Invoke(inventoryItemsToRemove);
                break;
            case < 0:
                throw new Exception("[20240815.0934.1] Inventory space went negative");
        }
    }

    public Dictionary ToDictionary() {
        Array itemsArray = new();
        foreach (StackedItems inventoryItems in _inventoryItemsList) {
            Dictionary dict = new() {
                { "Item", inventoryItems.Item.ToDictionary() },
                { "Count", inventoryItems.Count }
            };
            itemsArray.Add(dict);
        }

        Dictionary inventoryDict = new() {
            { "InventoryItemsList", itemsArray }
        };
        return inventoryDict;
    }
}