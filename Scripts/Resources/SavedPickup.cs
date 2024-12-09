using Godot;
using Godot.Collections;
namespace TerrariaRipoffNNF;

public partial class SavedPickup : Resource {
    public Vector2 Position { get; private set; }
    public InventoryItems InventoryItems { get; }
    public IntVector Indices { get; set; }

    public SavedPickup(Item item, Vector2 position, int count = 1) {
        InventoryItems = new InventoryItems(item, count);
        Position = position;
        Indices = new IntVector(position / Game.BlockSize);
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
        return new SavedPickup(inventoryItems.Item, position, inventoryItems.Count);
    }

    public SavedPickup() { }
}