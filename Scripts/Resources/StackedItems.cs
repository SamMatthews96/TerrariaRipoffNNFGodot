using Godot.Collections;

namespace TerrariaRipoffNNF;

public class StackedItems {
    public int Count { get; protected set; }
    public Item Item { get; protected init; }
    public float TotalSpace => Count * Item.InventorySpace;

    public StackedItems(Item item, int count = 1) {
        Item = item;
        Count = count;
    }
    
    public StackedItems ToStackedItems() {
        return this;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "ItemType", Item.ToDictionary() },
            { "Count", Count }
        };
    }

    public static InventoryItems Deserialize(Dictionary dictionary) {
        Item item = Item.FromDictionary(dictionary["ItemType"].AsGodotDictionary());
        int count = (int)dictionary["Count"];
        return new InventoryItems(item, count);
    }
}