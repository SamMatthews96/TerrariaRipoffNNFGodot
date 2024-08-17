using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 50;
    public float UsedSpace { get; private set; }

    private List<InventoryItems> _inventoryItemsList;
    public List<InventoryItems> InventoryItemsList => _inventoryItemsList;

    [Signal] public delegate void InventoryChangedEventHandler();

    public override void _Ready() {
        _inventoryItemsList = new List<InventoryItems>();
    }

    public bool TryAddItems(InventoryItems newInventoryItems) {
        if (newInventoryItems.TotalSpace > MaximumSpace - UsedSpace) return false;
        Rpc(nameof(ClientAddItems), newInventoryItems.Serialize());
        return true;
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

        EmitSignal(SignalName.InventoryChanged);

        // TryAddItems(newInventoryItems);
    }
}