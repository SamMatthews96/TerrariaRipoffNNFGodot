using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupArea : Area2D {
    [Export] private Player _player;
    
    public event Action<Pickup> TouchedItem;

    public void InitAsHost() {
        BodyEntered += OnCollidedWithPickup;
        TreeExiting += OnHostTreeExiting;
    }

    private void OnHostTreeExiting() {
        BodyEntered -= OnCollidedWithPickup;
        TreeExiting -= OnHostTreeExiting;
    }
    
    private void OnCollidedWithPickup(Node2D node) {
        if (node is Pickup activePickup) {
            TouchedItem?.Invoke(activePickup);
        } else {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }
    }
}