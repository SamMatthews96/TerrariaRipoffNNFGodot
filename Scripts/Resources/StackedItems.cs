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

    public StackedItems ToStackedItems() {
        return this;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "ItemType", Item.ToDictionary() },
            { "Count", Count }
        };
    }

    public static StackedItems Deserialize(Dictionary dictionary) {
        Item item = Item.FromDictionary(dictionary["ItemType"].AsGodotDictionary());
        int count = (int)dictionary["Count"];
        return new StackedItems(item, count);
    }
}