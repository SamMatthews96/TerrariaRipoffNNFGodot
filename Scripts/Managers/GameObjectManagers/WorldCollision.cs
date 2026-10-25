using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _collisionBlockScene;
    [Export] private int _observerRadius = 3;

    private PlayerManager _playerManager;
    private BlockManager _blockManager;
    private PickupManager _pickupManager;
    private Vector2I _worldSize;

    private readonly Dictionary<Vector2I, StaticBody2D> _activeCollisionBlocks = new();
    private readonly Dictionary<Vector2I, int> _observerCounts = new();

    public override void _Ready() {
        _playerManager = _world.PlayerManager;
        _blockManager = _world.BlockManager;
        _pickupManager = _world.PickupManager;
        _worldSize = _world.WorldSize;

        _playerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        _blockManager.BlockDestroyed += OnBlockDestroyed;
        _blockManager.BlockCreated += OnBlockCreated;
        TreeExiting += () => {
            _playerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
            _blockManager.BlockCreated -= OnBlockCreated;
            _blockManager.BlockDestroyed -= OnBlockDestroyed;
        };

        if (!_world.IsHost) return;
        _pickupManager.ServerPickupCreated += OnPickupCreated;
        _pickupManager.ServerPickupMoved += OnPickupMoved;
        _pickupManager.ServerPickupDestroyed += OnPickupDestroyed;
        TreeExiting += () => {
            _pickupManager.ServerPickupCreated -= OnPickupCreated;
            _pickupManager.ServerPickupMoved -= OnPickupMoved;
            _pickupManager.ServerPickupDestroyed -= OnPickupDestroyed;
        };
    }
    
    private void OnLocalPlayerSpawned(Player player) {
        IncrementObserverCounts(player.Coords);
        player.CellMoved += MoveObserver;
    }

    private void OnBlockCreated(Vector2I position) {
        if (_observerCounts.TryGetValue(position, out int count) && count > 0) {
            CreateCollisionBlock(position);
        }
    }

    private void OnBlockDestroyed(Vector2I position, ushort _) {
        RemoveCollisionBlock(position);
    }

    private void OnPickupCreated(Vector2I position) {
        IncrementObserverCounts(position);
    }

    private void OnPickupMoved(Vector2I newPosition, Vector2I oldPosition) {
        MoveObserver(newPosition, oldPosition);
    }
    
    private void OnPickupDestroyed(Vector2I position) {
        DecrementObserverCounts(position);
    }

    private void MoveObserver(Vector2I newPosition, Vector2I oldPosition) {
        IncrementObserverCounts(newPosition);
        DecrementObserverCounts(oldPosition);
    }

    private void IncrementObserverCounts(Vector2I position) {
        int startX = Mathf.Max(0, position.X - _observerRadius);
        int endX = Mathf.Min(_worldSize.X - 1, position.X + _observerRadius);
        int startY = Mathf.Max(0, position.Y - _observerRadius);
        int endY = Mathf.Min(_worldSize.Y - 1, position.Y + _observerRadius);

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                Vector2I cell = new(x, y);
                int count = _observerCounts.GetValueOrDefault(cell, 0);
                _observerCounts[cell] = ++count;
                if (count == 1 && HasBlockEntity(x, y)) {
                    CreateCollisionBlock(cell);
                }
            }
        }
    }

    private void DecrementObserverCounts(Vector2I position) {
        int startX = Mathf.Max(0, position.X - _observerRadius);
        int endX = Mathf.Min(_worldSize.X - 1, position.X + _observerRadius);
        int startY = Mathf.Max(0, position.Y - _observerRadius);
        int endY = Mathf.Min(_worldSize.Y - 1, position.Y + _observerRadius);

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                Vector2I cell = new(x, y);
                if (!_observerCounts.TryGetValue(cell, out int count)) continue;
                _observerCounts[cell] = --count;
                if (count == 0 && HasBlockEntity(x, y)) {
                    _observerCounts.Remove(cell);
                    RemoveCollisionBlock(cell);
                }
            }
        }
    }
    
    private bool HasBlockEntity(int x, int y) {
        return _blockManager.GetBlock(x, y) is not null;
    }

    private void CreateCollisionBlock(Vector2I position) {
        StaticBody2D block = _collisionBlockScene.Instantiate<StaticBody2D>();
        block.Position = position * Game.BlockSize;
        AddChild(block);

        _activeCollisionBlocks[position] = block;
    }

    private void RemoveCollisionBlock(Vector2I position) {
        if (_activeCollisionBlocks.TryGetValue(position, out StaticBody2D block)) {
            block.QueueFree();
            _activeCollisionBlocks.Remove(position);
        }
    }
}