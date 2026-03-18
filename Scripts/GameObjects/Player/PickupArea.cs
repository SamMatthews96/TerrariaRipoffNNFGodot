using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupArea : Area2D {
    private Player _player;
    
    // public event Action<WorldPickup> TouchedItem;
    
    public static PickupArea Create(Player player) {
        PickupArea pickupArea = Data.PackedScenes.PlayerPickupArea
            .Instantiate<PickupArea>();
        pickupArea._player = player;
        return pickupArea;
    }

    public override void _Ready() {
        BodyEntered += OnCollidedWithPickup;
    }
    
    public override void _ExitTree() {
        BodyEntered -= OnCollidedWithPickup;
    }

    private void OnCollidedWithPickup(Node2D node) {
        // if (node is WorldPickup activePickup) {
        //     TouchedItem?.Invoke(activePickup);
        // } else {
        //     throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        // }
    }
}