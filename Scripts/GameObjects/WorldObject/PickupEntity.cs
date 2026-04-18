using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupEntity : RigidBody2D {
    [Export] private Sprite2D _sprite;
    public Vector2I Coords;
    
    public Item Item { get; private set; }

    public static PickupEntity Create(string resourcePath, Vector2 worldPosition) {
        PickupEntity pickup = 
            Data.PackedScenes.Pickup.Instantiate<PickupEntity>();
        
        Item item = ResourceLoader.Load<Item>(resourcePath);
        if (item is null) {
            throw new Exception($"Failed to load item from {resourcePath}");
        }
        
        pickup.Item = item;
        pickup.Position = worldPosition;
        return pickup;
    }

    public override void _Ready() {
        _sprite.Texture = Item.IconTexture;
    }
}