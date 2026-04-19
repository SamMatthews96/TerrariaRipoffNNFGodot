using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ServerPickupArea : Area2D {
    private Player _player;
    
    public event Action<PickupEntity> PickupCollected;
    
    public static ServerPickupArea Create(Player player) {
        ServerPickupArea serverPickupArea = Data.PackedScenes.PlayerPickupArea
            .Instantiate<ServerPickupArea>();
        serverPickupArea._player = player;
        return serverPickupArea;
    }

    public override void _Ready() {
        BodyEntered += OnCollidedWithPickup;
    }
    
    public override void _ExitTree() {
        BodyEntered -= OnCollidedWithPickup;
    }

    private void OnCollidedWithPickup(Node2D node) {
        if (node is not PickupEntity pickup) return;
        // if (_player.Inventory.UsedSpace + pickup.Item.InventorySpace >
        //     _player.Inventory.MaximumSpace) return;
        PickupCollected?.Invoke(pickup);
    }
}