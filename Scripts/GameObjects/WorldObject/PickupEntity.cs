using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class PickupEntity : Node2D {
    [Export] private Sprite2D _sprite;
    public Item Item { get; private set; }

    public static PickupEntity Create(Item item, Vector2 worldPosition) {
        PickupEntity pickup = 
            Data.PackedScenes.Pickup.Instantiate<PickupEntity>();
        
        pickup.Item = item;
        pickup.Position = worldPosition;
        return pickup;
    }

    public override void _Ready() {
        _sprite.Texture = Item.IconTexture;
    }
}