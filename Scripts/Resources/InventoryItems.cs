using System;

namespace TerrariaRipoffNNF;

public class InventoryItems : StackedItems {
    public InventoryItems(Item item, int count = 1) : base(item, count) {
        Item = item;
        Count = count;
    }
    
    public static InventoryItems operator +(InventoryItems a, InventoryItems b) {
        return new InventoryItems(a.Item, a.Count + b.Count);
    }
    
    public static InventoryItems operator -(InventoryItems a, InventoryItems b) {
        return new InventoryItems(a.Item, a.Count - b.Count);
    }
}