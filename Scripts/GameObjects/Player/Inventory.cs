using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 50;
    public float UsedSpace { get; private set; }

    private List<InventoryItems> _inventoryItemsList;
    public List<InventoryItems> InventoryItemsList => _inventoryItemsList;

    public event Action InventoryChanged;
    public event Action<ActivePickup> PickedUpItem;

    [Export] private Player _player;

    public override void _Ready() {
        _inventoryItemsList = new List<InventoryItems>();

        if (Manager.Instance.Game.IsHost) {
            _player.PickupArea.BodyEntered += OnCollidedWithPickup;
        }
    }

    private void OnCollidedWithPickup(Node node) {
        if (node is not ActivePickup activePickup) {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }

        if (activePickup.SavedPickup.InventoryItems.TotalSpace > MaximumSpace - UsedSpace) {
            return;
        }

        Rpc(nameof(ClientAddItems), activePickup.SavedPickup.InventoryItems.Serialize());

        PickedUpItem?.Invoke(activePickup);
    }

    [Rpc(CallLocal = true)]
    private void ClientAddItems(Dictionary newInventoryItemsDictionary) {
        InventoryItems newInventoryItems =
            InventoryItems.Deserialize(newInventoryItemsDictionary);
        UsedSpace += newInventoryItems.TotalSpace;

        InventoryItems currentInventoryItems =
            _inventoryItemsList.Find(inventoryItems =>
                inventoryItems.ItemType == newInventoryItems.ItemType);

        if (currentInventoryItems == null) {
            _inventoryItemsList.Add(newInventoryItems);
        } else {
            currentInventoryItems.AddItems(newInventoryItems.Count);
        }

        InventoryChanged?.Invoke();
    }
}