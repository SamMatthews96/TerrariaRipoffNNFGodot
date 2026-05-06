using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.TestScenes;
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
    private ItemIdBimap _itemMap;

    public override void _Ready() {
        _itemMap = _player.World.ItemIdBimap;
        if (_player.IsLocalPlayer || _player.World.IsHost) {
            Godot.Collections.Dictionary<string, Array> inventory =
                _player.PlayerData["Inventory"].AsGodotDictionary<string, Array>();
            Array inventoryItems = inventory["InventoryItemsList"];

            foreach (Dictionary savedItem in inventoryItems) {
                Item newItem = Item.FromDictionary(savedItem["Item"].AsGodotDictionary());
                int count = (int)savedItem["Count"].ToString().ToFloat();
                StackedItems newStack = new(newItem, count);
                AddItems(newStack);
            }

            _player.Crafting.HostItemCrafted += OnHostItemCrafted;
            TreeExiting += () => { _player.Crafting.HostItemCrafted -= OnHostItemCrafted; };
        }

        if (_player.World.IsHost) {
            _player.ServerPickupArea.CollectedPickup += HostOnCollectedPickup;
            _player.ActionState.Build.HostPlacedBlock += HostOnPlacedBlock;
            _player.ActionState.Build.HostPlacedWall += HostOnPlacedBlock;
            _player.ActionState.Build.HostPlaceProp += HostOnPlacedBlock;
            TreeExiting += () => {
                _player.ServerPickupArea.CollectedPickup -= HostOnCollectedPickup;
                _player.ActionState.Build.HostPlacedBlock -= HostOnPlacedBlock;
                _player.ActionState.Build.HostPlacedWall -= HostOnPlacedBlock;
                _player.ActionState.Build.HostPlaceProp -= HostOnPlacedBlock;
            };
        }
    }

    private void OnHostItemCrafted(
        StackedItems newItems, Array<StackedItems> ingredients) {
        AddItems(newItems);
        foreach (StackedItems ingredient in ingredients) {
            RemoveItems(ingredient);
        }

        if (_player.PeerId == 1) return;

        ushort newItemId = _itemMap.GetId(newItems.Item);
        RpcId(_player.PeerId, nameof(RpcAddItems),
            newItemId, newItems.Count);

        foreach (StackedItems ingredient in ingredients) {
            ushort ingredientId = _itemMap.GetId(ingredient.Item);
            RpcId(_player.PeerId, nameof(RpcRemoveItems),
                ingredientId, ingredient.Count);
        }
    }

    private void HostOnPlacedBlock(Item item, Vector2I coords) {
        StackedItems inventoryItems = new(item);
        RemoveItems(inventoryItems);

        ushort itemId = _itemMap.GetId(item);
        if (_player.PeerId != 1) {
            RpcId(_player.PeerId, nameof(RpcRemoveItems),
                itemId, 1);
        }
    }

    private void OnItemActionClicked(StackedItems stackedItems) {
        if (stackedItems.Item.HasProperty<ItemEquipment>()) {
            EquipItemClicked?.Invoke(stackedItems.Item);
        }
    }

    private void HostOnCollectedPickup(PickupEntity pickup) {
        StackedItems stackedItems = new(pickup.Item);
        AddItems(stackedItems);
        if (_player.PeerId != 1) {
            ushort itemId = _itemMap.GetId(stackedItems.Item);
            RpcId(_player.PeerId, nameof(RpcAddItems),
                itemId, 1);
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcAddItems(ushort itemId, int count) {
        Item item = _itemMap.GetItem(itemId);
        StackedItems inventoryItems = new(item, count);
        AddItems(inventoryItems);
    }

    private void AddItems(StackedItems itemsToAdd) {
        UsedSpace += itemsToAdd.TotalSpace;

        int index = _inventoryItemsList.FindIndex(items =>
            Item.AreEqual(items.Item, itemsToAdd.Item));

        if (index == -1) {
            _inventoryItemsList.Add(itemsToAdd);
            AddedItemStack?.Invoke(itemsToAdd);
        } else {
            _inventoryItemsList[index] += itemsToAdd;
            ItemStackChangedSize?.Invoke(_inventoryItemsList[index]);
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRemoveItems(ushort itemId, int count) {
        Item item = _itemMap.GetItem(itemId);
        StackedItems inventoryItems = new(item, count);
        RemoveItems(inventoryItems);
    }

    private void RemoveItems(StackedItems itemsToRemove) {
        UsedSpace -= itemsToRemove.TotalSpace;

        int index = _inventoryItemsList.FindIndex(inventoryItems =>
            _itemMap.AreItemsSame(inventoryItems.Item, itemsToRemove.Item
            ));

        if (index == -1) {
            throw new Exception("[20240815.0934.1] Inventory item not found");
        }

        _inventoryItemsList[index] -= itemsToRemove;

        switch (_inventoryItemsList[index].Count) {
            case > 0:
                ItemStackChangedSize?.Invoke(_inventoryItemsList[index]);
                break;
            case 0:
                _inventoryItemsList.RemoveAt(index);
                RemovedItemStack?.Invoke(itemsToRemove);
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