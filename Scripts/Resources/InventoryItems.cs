using System;

namespace TerrariaRipoffNNF;

public class InventoryItems : StackedItems {
    public InventoryItems(Item item, int count = 1) : base(item, count) {
        Item = item;
        Count = count;
    }
    
    public static InventoryItems operator +(InventoryItems a, InventoryItems b) {
        if (a.Item != b.Item) {
            throw new Exception("[20240816.0934.1] Attempted to add different item types");
        }
        
        return new InventoryItems(a.Item, a.Count + b.Count);
    }
    
    public static InventoryItems operator -(InventoryItems a, InventoryItems b) {
        if (a.Item != b.Item) {
            throw new Exception("[20240816.0934.1] Attempted to subtract different item types");
        }
        
        return new InventoryItems(a.Item, a.Count - b.Count);
    }
    
    
}