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
    public event Action<Pickup> PickedUpItem;

    public event Action<Item> EquipItemClicked;

    [Export] private Player _player;
    private Game _game;
    private readonly List<StackedItems> _inventoryItemsList = new();

    public void InitAsHost() {
        _player.PickupArea.TouchedItem += OnHostCollidedWithPickup;
        _player.ActionController.BuildAction.BuildActionAttempted += OnBuildActionAttempted;
        TreeExiting += () => {
            _player.PickupArea.TouchedItem -= OnHostCollidedWithPickup;
            _player.ActionController.BuildAction.BuildActionAttempted -= OnBuildActionAttempted;
        };
    }

    public void InitAsLocal(Game game, Dictionary playerData) {
        _game = game;
        _game.Interface.InventoryUi.ItemActionClicked += OnItemActionClicked;

        TreeExiting += () => { _game.Interface.InventoryUi.ItemActionClicked -= OnItemActionClicked; };

        if (!playerData.TryGetValue("Inventory", out Variant inventory)) return;
        if (!inventory.AsGodotDictionary<string, Array>().TryGetValue(
                "InventoryItemsList", out Array inventoryItems)) return;

        foreach (Dictionary savedItem in inventoryItems) {
            StackedItems newItem = new (
                Item.FromDictionary(savedItem["Item"].AsGodotDictionary()),
                (int)savedItem["Count"].ToString().ToFloat()
            );
            ClientAddItems(newItem);
        }
    }

    private void OnItemActionClicked(StackedItems stackedItems) {
        if (stackedItems.Item.HasProperty<ItemEquipment>()) {
            EquipItemClicked?.Invoke(stackedItems.Item);
        }
    }

    public override void _Ready() {
        _player.Crafting.ItemCrafted += OnItemCrafted;
    }

    private void OnItemCrafted(StackedItems newItems, List<StackedItems> ingredients) {
        Rpc(nameof(ClientAddItems), newItems);
        foreach (StackedItems ingredient in ingredients) {
            Rpc(nameof(ClientRemoveItems), ingredient);
        }
    }

    private void OnHostCollidedWithPickup(Pickup pickup) {
        if (pickup.Items.TotalSpace > MaximumSpace - UsedSpace) {
            return;
        }

        Rpc(nameof(ClientAddItems), pickup.Items);

        PickedUpItem?.Invoke(pickup);
    }

    private void OnBuildActionAttempted(Item item, IntVector _) {
        StackedItems inventoryItems = new(item, 1);
        Rpc(nameof(ClientRemoveItems), inventoryItems);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ClientAddItems(StackedItems inventoryItemsToAdd) {
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ClientRemoveItems(StackedItems inventoryItemsToRemove) {
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