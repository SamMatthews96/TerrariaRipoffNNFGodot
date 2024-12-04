using Godot.Collections;

namespace TerrariaRipoffNNF;

public class StackedItems {
    public int Count { get; protected set; }
    public ItemType ItemType { get; protected init; }
    public float TotalSpace => Count * ItemType.InventorySpace;

    public StackedItems(ItemType itemType, int count = 1) {
        ItemType = itemType;
        Count = count;
    }
    
    public StackedItems ToStackedItems() {
        return this;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "ItemType", ItemType.Serialize() },
            { "Count", Count }
        };
    }

    public static InventoryItems Deserialize(Dictionary dictionary) {
        ItemType itemType = ItemType.Deserialize(dictionary["ItemType"].AsGodotDictionary());
        int count = (int)dictionary["Count"];
        return new InventoryItems(itemType, count);
    }
}