using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 50;
    public float UsedSpace { get; private set; }

    public List<StackedItems> StackedItemsList =>
        _inventoryItemsList.ConvertAll(inventoryItems => inventoryItems.ToStackedItems());

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
    public event Action<ActivePickup> PickedUpItem;

    [Export] private Player _player;

    private readonly List<InventoryItems> _inventoryItemsList = new();

    public void InitAsHost() {
        _player.PickupArea.TouchedItem += OnHostCollidedWithPickup;
        _player.ActionController.BuildAction.BlockPlaced += OnBlockPlaced;
        TreeExiting += OnHostTreeExiting;
    }

    private void OnHostTreeExiting() {
        _player.PickupArea.TouchedItem -= OnHostCollidedWithPickup;
        _player.ActionController.BuildAction.BlockPlaced -= OnBlockPlaced;
        TreeExiting -= OnHostTreeExiting;
    }


    public override void _Ready() {
        //@todo remove placeholder items
        Item tempMetal = ResourceLoader.Load<Item>("res://Resources/Items/Ferrium.tres");
        InventoryItems metalItems = new(tempMetal, 10);
        ClientAddItems(metalItems.Serialize());
        Item tempWood = ResourceLoader.Load<Item>("res://Resources/Items/wood1.tres");
        InventoryItems woodItems = new(tempWood, 10);
        ClientAddItems(woodItems.Serialize());

        _player.Crafting.ItemCrafted += OnItemCrafted;
    }

    private void OnItemCrafted(StackedItems newItems, List<StackedItems> ingredients) {
        Rpc(nameof(ClientAddItems), newItems.Serialize());
        foreach (StackedItems ingredient in ingredients) {
            Rpc(nameof(ClientRemoveItems), ingredient.Serialize());
        }
    }

    private void OnHostCollidedWithPickup(ActivePickup activePickup) {
        if (activePickup.SavedPickup.InventoryItems.TotalSpace > MaximumSpace - UsedSpace) {
            return;
        }

        Rpc(nameof(ClientAddItems), activePickup.SavedPickup.InventoryItems.Serialize());

        PickedUpItem?.Invoke(activePickup);
    }

    private void OnBlockPlaced(Item item, IntVector _) {
        InventoryItems inventoryItems = new(item, 1);
        Dictionary inventoryItemsDictionary = inventoryItems.Serialize();
        Rpc(nameof(ClientRemoveItems), inventoryItemsDictionary);
    }

    [Rpc(CallLocal = true)]
    private void ClientAddItems(Dictionary inventoryItemsDictionary) {
        InventoryItems inventoryItemsToAdd =
            StackedItems.Deserialize(inventoryItemsDictionary);
        UsedSpace += inventoryItemsToAdd.TotalSpace;

        int index = _inventoryItemsList.FindIndex(inventoryItems =>
            inventoryItems.Item == inventoryItemsToAdd.Item);

        if (index == -1) {
            _inventoryItemsList.Add(inventoryItemsToAdd);
            AddedItemStack?.Invoke(inventoryItemsToAdd);
        } else {
            _inventoryItemsList[index] += inventoryItemsToAdd;
            ItemStackChangedSize?.Invoke(_inventoryItemsList[index].ToStackedItems());
        }
    }

    [Rpc(CallLocal = true)]
    private void ClientRemoveItems(Dictionary inventoryItemsDictionary) {
        InventoryItems inventoryItemsToRemove =
            StackedItems.Deserialize(inventoryItemsDictionary);
        UsedSpace -= inventoryItemsToRemove.TotalSpace;

        int index = _inventoryItemsList.FindIndex(inventoryItems =>
            inventoryItems.Item == inventoryItemsToRemove.Item);

        if (index == -1) {
            throw new Exception("[20240815.0934.1] Inventory item not found");
        }

        _inventoryItemsList[index] -= inventoryItemsToRemove;

        switch (_inventoryItemsList[index].Count) {
            case > 0:
                ItemStackChangedSize?.Invoke(_inventoryItemsList[index].ToStackedItems());
                break;
            case 0:
                _inventoryItemsList.RemoveAt(index);
                RemovedItemStack?.Invoke(inventoryItemsToRemove);
                break;
            case < 0:
                throw new Exception("[20240815.0934.1] Inventory space went negative");
        }
    }
}