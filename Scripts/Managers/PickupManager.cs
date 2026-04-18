using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;

    public override void _Ready() {
        _world.BlockDestroyed += OnBlockDestroyed;
    }

    public override void _ExitTree() {
        _world.BlockDestroyed -= OnBlockDestroyed;
    }

    private void OnBlockDestroyed(Vector2I coords, string resourcePath) {
        Vector2 worldPosition = new(
            coords.X * Game.BlockSize + Game.BlockSize / 2f,
            coords.Y * Game.BlockSize + Game.BlockSize / 2f
        );

        Item item = ResourceLoader.Load<Item>(resourcePath);
        if (item is null) {
            throw new Exception($"Failed to load item from {resourcePath}");
        }

        PickupEntity pickup = PickupEntity.Create(item, worldPosition);
        _activePickups.Add(pickup);
        AddChild(pickup);
    }

    public override void _PhysicsProcess(double delta) {
        foreach (PickupEntity pickup in _activePickups) {
            
        }
    }
}