using Godot;

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
    
}