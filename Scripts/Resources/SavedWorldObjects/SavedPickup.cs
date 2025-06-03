using Godot;
using Godot.Collections;
namespace TerrariaRipoffNNF;

public partial class SavedPickup : SavedWorldObject {
    public Vector2 Position { get; private set; }
    public InventoryItems InventoryItems { get; }

    public SavedPickup(Item item, Vector2 position, int count = 1) {
        InventoryItems = new InventoryItems(item, count);
        Position = position;
        XPosition = (int)(position.X / Game.BlockSize);
        YPosition = (int)(position.Y / Game.BlockSize);
    }

    public override Dictionary ToDictionary() {
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
}