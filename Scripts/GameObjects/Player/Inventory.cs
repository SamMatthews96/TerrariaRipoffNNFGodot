using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

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

    public event Action<Item> EquipItemClicked;

    [Export] private Player _player;
    private Game _game;
    private readonly List<InventoryItems> _inventoryItemsList = new();

    public void InitAsHost() {
        _player.PickupArea.TouchedItem += OnHostCollidedWithPickup;
        _player.ActionController.BuildAction.BlockPlaced += OnBlockPlaced;
        TreeExiting += () => {
            _player.PickupArea.TouchedItem -= OnHostCollidedWithPickup;
            _player.ActionController.BuildAction.BlockPlaced -= OnBlockPlaced;
        };
    }

    public void InitAsLocal(Game game, Dictionary playerData) {
        _game = game;
        _game.Interface.InventoryUi.ItemActionClicked += OnItemActionClicked;

        TreeExiting += () => { _game.Interface.InventoryUi.ItemActionClicked -= OnItemActionClicked; };

        if (!playerData.TryGetValue("Inventory", out Variant inventory)) return;
        if (!inventory.AsGodotDictionary<string,Array>().TryGetValue(
            "InventoryItemsList", out Array inventoryItems)) return;
        
        foreach (Dictionary savedItem in inventoryItems) {
            ClientAddItems(savedItem);
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
        Rpc(nameof(ClientAddItems), newItems.Serialize());
        foreach (StackedItems ingredient in ingredients) {
            Rpc(nameof(ClientRemoveItems), ingredient.Serialize());
        }
    }

    private void OnHostCollidedWithPickup(ActivePickup activePickup) {
        // if (activePickup.SavedPickup.InventoryItems.TotalSpace > MaximumSpace - UsedSpace) {
        //     return;
        // }

        // Rpc(nameof(ClientAddItems), activePickup.SavedPickup.InventoryItems.Serialize());

        PickedUpItem?.Invoke(activePickup);
    }

    private void OnBlockPlaced(Item item, IntVector _) {
        InventoryItems inventoryItems = new(item, 1);
        Dictionary inventoryItemsDictionary = inventoryItems.Serialize();
        Rpc(nameof(ClientRemoveItems), inventoryItemsDictionary);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
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

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
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

    public Dictionary Serialize() {
        Array itemsArray = new();
        foreach (InventoryItems inventoryItems in _inventoryItemsList) {
            itemsArray.Add(inventoryItems.Serialize());
        }

        Dictionary inventoryDict = new() {
            // { "MaximumSpace", MaximumSpace },
            // { "UsedSpace", UsedSpace },
            { "InventoryItemsList", itemsArray }
        };
        return inventoryDict;
    }
}