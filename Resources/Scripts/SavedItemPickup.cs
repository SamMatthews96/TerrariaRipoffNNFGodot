using System;
using Godot;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class SavedItemPickup : Resource, ISavedGameObject {
    private bool _isActive = true;
    public Vector2 Position { get; private set; }
    public int Count { get; private set; }
    public ActiveItemPickup ActiveItemPickup { get; private set; }

    public IntVector GridPosition => new(
        (int)Math.Round(Position.X / BlockManager.BlockSize),
        (int)Math.Round(Position.Y / BlockManager.BlockSize));

    public InventoryItemType InventoryItemType { get; private set; }

    public SavedItemPickup(InventoryItemType inventoryItemType, Vector2 position, int count = 1) {
        InventoryItemType = inventoryItemType;
        Position = position;
        Count = count;
    }
}