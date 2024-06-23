using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class ActiveItemPickup : Node2D {
    private static readonly PackedScene PackedScene =
        (ResourceLoader.Load<PackedScene>("res://GameObjects/Scenes/ActiveItemPickup.tscn"));

    [Export] private Sprite2D _sprite;
    private int _xPosition;
    private int _yPosition;
    private InventoryItemType _inventoryItemType;


    public static ActiveItemPickup Initialize(SavedItemPickup savedItemPickup) {
        ActiveItemPickup newItemPickup = PackedScene.Instantiate<ActiveItemPickup>();

        newItemPickup._inventoryItemType = savedItemPickup.InventoryItemType;
        newItemPickup._xPosition = savedItemPickup.GridPosition.X;
        newItemPickup._yPosition = savedItemPickup.GridPosition.Y;
        newItemPickup.Position = savedItemPickup.Position;

        newItemPickup._sprite.Texture = savedItemPickup.InventoryItemType.IconTexture;
        
        return newItemPickup;
    }
    
}