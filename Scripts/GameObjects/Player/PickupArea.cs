using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PickupArea : Area2D {
    public event Action<ActivePickup> TouchedItem;

    public override void _Ready() {
        if (!SceneManager.Instance.Game.IsHost) return;

        BodyEntered += OnCollidedWithPickup;
    }

    public override void _ExitTree() {
        if (!SceneManager.Instance.Game.IsHost) return;

        BodyEntered -= OnCollidedWithPickup;
    }

    private void OnCollidedWithPickup(Node2D node) {
        if (node is ActivePickup activePickup) {
            TouchedItem?.Invoke(activePickup);
        } else {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }
    }
}