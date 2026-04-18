using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;
    private int _pickupCount = 0;

    public override void _Ready() {
        if (!Multiplayer.IsServer()) return;
        _world.BlockDestroyed += ServerOnBlockDestroyed;
    }

    public override void _ExitTree() {
        if (!Multiplayer.IsServer()) return;
        _world.BlockDestroyed -= ServerOnBlockDestroyed;
    }

    private void ServerOnBlockDestroyed(Vector2I coords, string resourcePath) {
        if (!Multiplayer.IsServer()) return;
        
        Vector2 position = new(
            (coords.X + 0.5f) * Game.BlockSize,
            (coords.Y + 0.5f) * Game.BlockSize
        );
        _pickupCount++;
        string name = $"Pickup{_pickupCount}";

        Rpc(nameof(RpcAllCreatePickup), position, resourcePath, name);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreatePickup(Vector2 position, string resourcePath, string name) {
        PickupEntity pickup = 
            Data.PackedScenes.Pickup.Instantiate<PickupEntity>();
        Item item = ResourceLoader.Load<Item>(resourcePath);
        pickup.Position = position;
        pickup.Item = item;
        pickup.Name = name;
        AddChild(pickup);
        _activePickups.Add(pickup);
    }

    public override void _PhysicsProcess(double delta) {
        foreach (PickupEntity pickup in _activePickups) {
        }
    }
}