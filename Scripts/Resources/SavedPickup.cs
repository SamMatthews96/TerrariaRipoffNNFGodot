using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class SavedPickup : Resource {
    public Vector2 Position { get; private set; }
    public InventoryItems InventoryItems { get; private set; }

    public SavedPickup(ItemType itemType, Vector2 position, int count = 1) {
        InventoryItems = new InventoryItems(itemType, count);
        Position = position;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "InventoryItemType", InventoryItems.Serialize() },
            { "Position", Position },
        };
    }

    public static SavedPickup Deserialize(Dictionary dictionary) {
        InventoryItems inventoryItems =
            InventoryItems.Deserialize(dictionary["InventoryItemType"].AsGodotDictionary());
        Vector2 position = (Vector2)dictionary["Position"];
        return new SavedPickup(inventoryItems.ItemType, position, inventoryItems.Count);
    }

    public SavedPickup() { }
}