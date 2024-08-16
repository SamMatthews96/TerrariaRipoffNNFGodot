using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Utils;
using GameManager = TerrariaRipoffNNF.Scripts.Managers.GameManager;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class SavedPickup : Resource {
    public Vector2 Position { get; private set; }
    public int Count { get; private set; }
    public ActivePickup ActivePickup { get; private set; }

    public IntVector GridPosition => new(
        (int)Math.Round(Position.X / GameManager.BlockSize),
        (int)Math.Round(Position.Y / GameManager.BlockSize));

    public ItemType ItemType { get; private set; }

    public SavedPickup(ItemType itemType, Vector2 position, int count = 1) {
        ItemType = itemType;
        Position = position;
        Count = count;
    }

    public Dictionary Serialize() {
        return new Dictionary {
            { "InventoryItemType", ItemType.Serialize() },
            { "Position", Position },
            { "Count", Count }
        };
    }

    public static SavedPickup Deserialize(Dictionary dictionary) {
        ItemType itemType = ItemType.Deserialize(dictionary["InventoryItemType"].AsGodotDictionary());
        Vector2 position = (Vector2)dictionary["Position"];
        int count = (int)dictionary["Count"];
        return new SavedPickup(itemType, position, count);
    }

    public SavedPickup() { }
}