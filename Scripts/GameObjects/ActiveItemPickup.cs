using Godot;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActiveItemPickup : RigidBody2D {
    private static readonly PackedScene PackedScene =
        (ResourceLoader.Load<PackedScene>("res://GameObjects/Scenes/ActiveItemPickup.tscn"));

    [Export] private Sprite2D _sprite;
    private SavedItemPickup _savedItemPickup;

    public static ActiveItemPickup Initialize(SavedItemPickup savedItemPickup) {
        ActiveItemPickup newItemPickup = PackedScene.Instantiate<ActiveItemPickup>();

        newItemPickup._savedItemPickup = savedItemPickup;
        newItemPickup.Position = savedItemPickup.Position;

        newItemPickup._sprite.Texture = savedItemPickup.InventoryItemType.IconTexture;
         
        return newItemPickup;
    }

    public override void _Process(double delta) {
    }
}