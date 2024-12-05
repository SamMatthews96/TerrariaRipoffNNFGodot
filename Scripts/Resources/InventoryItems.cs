using System;

namespace TerrariaRipoffNNF;

public class InventoryItems : StackedItems {
    public InventoryItems(ItemType itemType, int count = 1) : base(itemType, count) {
        ItemType = itemType;
        Count = count;
    }
    
    public static InventoryItems operator +(InventoryItems a, InventoryItems b) {
        if (a.ItemType != b.ItemType) {
            throw new Exception("[20240816.0934.1] Attempted to add different item types");
        }
        
        return new InventoryItems(a.ItemType, a.Count + b.Count);
    }
    
    public static InventoryItems operator -(InventoryItems a, InventoryItems b) {
        if (a.ItemType != b.ItemType) {
            throw new Exception("[20240816.0934.1] Attempted to subtract different item types");
        }
        
        return new InventoryItems(a.ItemType, a.Count - b.Count);
    }
    
    
}