using Godot;
using System.Collections.Generic;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node {
    [Export] private PackedScene _collisionBlock;

    [Export] private World _world;
    private StaticBody2D[,] _activeCollisionBlocks;

    public override void _Ready() {
        _activeCollisionBlocks = new StaticBody2D[_world.WorldSize.X, _world.WorldSize.Y];
    }

    public void UpdateCollisionBlocks(Vector2I playerPosition, int radius) {
        int startX = Mathf.Max(0, playerPosition.X - radius);
        int endX = Mathf.Min(_world.WorldSize.X - 1, playerPosition.X + radius);
        int startY = Mathf.Max(0, playerPosition.Y - radius);
        int endY = Mathf.Min(_world.WorldSize.Y - 1, playerPosition.Y + radius);

        // Create collision blocks within radius where blocks exist
        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                if (_activeCollisionBlocks[x, y] == null && HasBlockEntity(x, y)) {
                    CreateCollisionBlock(x, y);
                }
            }
        }
    }

    private bool HasBlockEntity(int x, int y) {
        var entities = _world.Entities[x, y];
        if (entities == null) return false;

        foreach (var entity in entities) {
            if (entity is BlockEntity) {
                return true;
            }
        }

        return false;
    }

    private void CreateCollisionBlock(int x, int y) {
        if (_collisionBlock == null) return;

        var block = _collisionBlock.Instantiate<StaticBody2D>();
        block.Position = new Vector2(x * Game.BlockSize, y * Game.BlockSize);
        AddChild(block);

        _activeCollisionBlocks[x, y] = block;
    }

    public void RemoveCollisionBlockAt(int x, int y) {
        if (x >= 0 && x < _activeCollisionBlocks.GetLength(0) &&
            y >= 0 && y < _activeCollisionBlocks.GetLength(1) &&
            _activeCollisionBlocks[x, y] != null) {
            _activeCollisionBlocks[x, y].QueueFree();
            _activeCollisionBlocks[x, y] = null;
        }
    }

    public void ClearAllCollisionBlocks() {
        for (int x = 0; x < _activeCollisionBlocks.GetLength(0); x++) {
            for (int y = 0; y < _activeCollisionBlocks.GetLength(1); y++) {
                if (_activeCollisionBlocks[x, y] != null) {
                    _activeCollisionBlocks[x, y].QueueFree();
                    _activeCollisionBlocks[x, y] = null;
                }
            }
        }
    }
}
