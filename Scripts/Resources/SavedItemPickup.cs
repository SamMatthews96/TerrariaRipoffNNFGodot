using System;
using Godot;
using TerrariaRipoffNNF.Scripts.Utils;
using ActiveItemPickup = TerrariaRipoffNNF.Scripts.GameObjects.ActiveItemPickup;
using GameManager = TerrariaRipoffNNF.Scripts.Managers.GameManager;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class SavedItemPickup : Resource, ISavedGameObject {
    private bool _isActive = true;
    public Vector2 Position { get; private set; }
    public int Count { get; private set; }
    public ActiveItemPickup ActiveItemPickup { get; private set; }

    public IntVector GridPosition => new(
        (int)Math.Round(Position.X / GameManager.BlockSize),
        (int)Math.Round(Position.Y / GameManager.BlockSize));

    public InventoryItemType InventoryItemType { get; private set; }

    public SavedItemPickup(InventoryItemType inventoryItemType, Vector2 position, int count = 1) {
        InventoryItemType = inventoryItemType;
        Position = position;
        Count = count;
    }
}