using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupArea : Area2D {
    [Export] private Player _player;
    
    public event Action<ActivePickup> PickedUpItem;

    public override void _Ready() {
        if (Manager.Instance.Game.IsHost) {
            BodyEntered += OnCollidedWithPickup;
        }
    }
    
    private void OnCollidedWithPickup(Node node) {
        if (node is not ActivePickup activePickup) {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }

        bool success = _player.Inventory.TryAddItems(activePickup.SavedPickup.InventoryItems);
        if (success) {
            PickedUpItem?.Invoke(activePickup);
        }
    }
}