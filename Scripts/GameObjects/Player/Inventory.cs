using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 100;
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
    public event Action<Item> EquipItemClicked;

    [Export] private Player _player;
    private readonly List<StackedItems> _inventoryItemsList = new();

    public override void _Ready() {
        if (_player.IsLocalPlayer) {
            Godot.Collections.Dictionary<string, Array> inventory = 
                _player.PlayerData["Inventory"].AsGodotDictionary<string, Array>();
            Array inventoryItems = inventory["InventoryItemsList"];
            
            foreach (Dictionary savedItem in inventoryItems) {
                Item newItem = Item.FromDictionary(savedItem["Item"].AsGodotDictionary());
                int count = (int)savedItem["Count"].ToString().ToFloat();
                StackedItems newStack = new(newItem, count);
                AddItems(newStack);
            }
        }

        if (_player.World.IsHost) {
            _player.ServerPickupArea.CollectedPickup += HostOnCollectedPickup;
            TreeExiting += () => {
                _player.ServerPickupArea.CollectedPickup -= HostOnCollectedPickup;
            };
        }
        
        // _player.Crafting.ItemCrafted += OnItemCrafted;
        // _player.World.Interface.InventoryUi.ItemActionClicked += OnItemActionClicked;
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

    public void OnAfterBuildSuccess(Item item) {
        StackedItems inventoryItems = new(item, 1);
        RemoveItems(inventoryItems);
    }

    private void HostOnCollectedPickup(PickupEntity pickup) {
        StackedItems stackedItems = new(pickup.Item);
        AddItems(stackedItems);
        if (_player.PeerId != 1) {
            Dictionary stackedItemsDict = stackedItems.ToDictionary();
            RpcId(_player.PeerId, nameof(RpcAddItems), stackedItemsDict);
        }
    }

    [Rpc]
    private void RpcAddItems(Dictionary stackedItemsDict) {
        StackedItems inventoryItems = StackedItems.FromDictionary(stackedItemsDict);
        AddItems(inventoryItems);
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
            Item.AreEqual(inventoryItems.Item,inventoryItemsToRemove.Item));

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