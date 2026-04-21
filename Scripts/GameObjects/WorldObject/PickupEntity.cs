using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupEntity : RigidBody2D {
    [Export] private Sprite2D _sprite;
    public Vector2I Coords;
    public Item Item;

    public override void _Ready() {
        _sprite.Texture = Item.IconTexture;
    }

    public void QueueFreeAllPeers() {
        Rpc(nameof(RpcAllDestroy));
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroy() {
        QueueFree();
    }
    
}