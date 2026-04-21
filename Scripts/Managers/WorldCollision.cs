using System.Collections.Generic;
using Godot;
using GodotCollections = Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _collisionBlockScene;
    [Export] private int _observerRadius = 3;

    // private Block[,] _blocks;
    private readonly Dictionary<Vector2I, StaticBody2D> _activeCollisionBlocks = new();
    private readonly Dictionary<Vector2I, int> _observerCounts = new(); 
    private Vector2I _worldSize;

    public override void _Ready() {
        // _blocks = _world.Blocks;
        _worldSize = _world.WorldSize;
        _world.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        
        if (Multiplayer.IsServer()) {
            InitAsHost();
        }
    }
    
    public override void _ExitTree() {
        _world.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void InitAsHost() {
        _world.BlockDestroyed += OnBlockDestroyed;
        _world.BlockCreated += OnBlockCreated;
        _world.PickupManager.ServerPickupCreated += OnPickupCreated;
        _world.PickupManager.ServerPickupMoved += OnPickupMoved;
        _world.PickupManager.ServerPickupDestroyed += OnPickupDestroyed;
        TreeExiting += HostOnTreeExiting;
    }
    
    private void HostOnTreeExiting() {
        TreeExiting -= HostOnTreeExiting;
        _world.BlockDestroyed -= OnBlockDestroyed;
        _world.BlockCreated -= OnBlockCreated;
        _world.PickupManager.ServerPickupCreated -= OnPickupCreated;
        _world.PickupManager.ServerPickupMoved -= OnPickupMoved;
        _world.PickupManager.ServerPickupDestroyed -= OnPickupDestroyed;
    }

    private void OnLocalPlayerSpawned(Player player) {
        IncrementObserverCounts(player.Coords);
        player.LocalPlayerMovedCell += MoveObserver;
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

    private void OnBlockCreated(Vector2I position) {
        if (_observerCounts.TryGetValue(position, out int count) && count > 0) {
            CreateCollisionBlock(position);
        }
    }

    private void OnBlockDestroyed(Vector2I position, string _) {
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
    
    private bool HasBlockEntity(int x, int y) {
        return _world.Blocks[x, y] != null;
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