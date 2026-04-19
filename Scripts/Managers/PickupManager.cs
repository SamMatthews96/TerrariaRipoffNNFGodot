using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;
    private int _pickupCount;

    public event Action<Vector2I> ServerPickupCreated;
    public delegate void CellMovedDelegate(Vector2I newCoords, Vector2I oldCoords);
    public event CellMovedDelegate ServerPickupMoved;
    public event Action<Vector2I> ServerPickupDestroyed;

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
        ServerPickupCreated?.Invoke(coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreatePickup(Vector2 position, string resourcePath, string name) {
        PickupEntity pickup = 
            Data.PackedScenes.Pickup.Instantiate<PickupEntity>();
        Item item = ResourceLoader.Load<Item>(resourcePath);
        pickup.Position = position;
        pickup.Coords = new Vector2I(
            (int)(position.X / Game.BlockSize - 0.5f),
            (int)(position.Y / Game.BlockSize - 0.5f)
        );
        pickup.Item = item;
        pickup.Name = name;
        AddChild(pickup);
        _activePickups.Add(pickup);
    }

    public override void _PhysicsProcess(double delta) {
        if (!Multiplayer.IsServer()) return;
        
        foreach (PickupEntity pickup in _activePickups) {
            Vector2I newCoords = new(
                (int)(pickup.Position.X / Game.BlockSize - 0.5f),
                (int)(pickup.Position.Y / Game.BlockSize - 0.5f)
            );
            if (pickup.Coords == newCoords) continue;
            
            ServerPickupMoved?.Invoke(newCoords, pickup.Coords);
            pickup.Coords = newCoords;
        }
    }
}