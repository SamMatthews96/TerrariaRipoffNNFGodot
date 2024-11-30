using Godot.Collections;

namespace TerrariaRipoffNNF;

public class InventoryItems {
    public int Count { get; private set; }
    public ItemType ItemType { get; }
    public float TotalSpace => Count * ItemType.InventorySpace;
    
    public InventoryItems(ItemType itemType, int count) {
        ItemType = itemType;
        Count = count;
    }

    public void AddItems(int count) {
        Count += count;
    }

    public void RemoveItems(int count) {
        Count -= count;
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