using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class Inventory : Node {
    public float MaximumSpace { get; private set; } = 50;
    public float UsedSpace { get; private set; }

    private List<InventoryItems> _inventoryItemsList;

    public override void _Ready() {
        _inventoryItemsList = new List<InventoryItems>();
    }

    public bool TryAddItems(InventoryItems newInventoryItems) {
        if (newInventoryItems.TotalSpace > MaximumSpace - UsedSpace) return false;
        UsedSpace += newInventoryItems.TotalSpace;

        InventoryItems currentInventoryItems =
            _inventoryItemsList.Find(inventoryItems =>
                inventoryItems.ItemType == newInventoryItems.ItemType);

        if (currentInventoryItems == null) {
            _inventoryItemsList.Add(newInventoryItems);
        } else {
            currentInventoryItems.AddItems(newInventoryItems.Count);
        }

        return true;
    }
}