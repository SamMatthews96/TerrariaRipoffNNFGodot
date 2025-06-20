using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class StackedItems : Resource {
    public int Count { get; protected set; }
    public Item Item { get; protected init; }
    public float TotalSpace => Count * Item.InventorySpace;

    public StackedItems(Item item, int count = 1) {
        Item = item;
        Count = count;
    }
    
    public static StackedItems operator +(StackedItems a, StackedItems b) {
        return new StackedItems(a.Item, a.Count + b.Count);
    }
    
    public static StackedItems operator -(StackedItems a, StackedItems b) {
        return new StackedItems(a.Item, a.Count - b.Count);
    }

    public StackedItems() { }
    
    public static StackedItems FromDictionary(Dictionary dictionary) {
        if (!dictionary.TryGetValue("Item", out Variant itemVariant)) {
            throw new Exception("[20250615.1109.2] Item not found in dictionary.");
        }
        
        if (!dictionary.TryGetValue("Count", out Variant countVariant)) {
            throw new Exception("[20250615.1109.3] Count not found in dictionary.");
        }
        
        Item item = Item.FromDictionary(itemVariant.AsGodotDictionary());
        int count = (int)countVariant.ToString().ToFloat();
        
        return new StackedItems(item, count);
    }
    
    public Dictionary ToDictionary() {
        return new Dictionary {
            { "Item", Item.ToDictionary() },
            { "Count", Count.ToString() }
        };
    }
    
}