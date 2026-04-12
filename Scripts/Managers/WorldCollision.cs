using System.Collections.Generic;
using System.Linq;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node2D {
    // when blocks are spawned / despawned, this needs to check
    
    [Export] private World _world;
    [Export] private PackedScene _collisionBlockScene;
    
    private List<IEntity>[,] _entities;
    private StaticBody2D[,] _activeCollisionBlocks;
    private Vector2I _worldSize;

    public void Init(List<IEntity>[,] entities, Vector2I worldSize) {
        _entities = entities;
        _worldSize = worldSize;
        _activeCollisionBlocks = new StaticBody2D[worldSize.X, worldSize.Y];
    }

    public override void _Ready() {
        _world.BlockDestroyed += OnBlockDestoyed;
    }
    
    public override void _ExitTree() {
        _world.BlockDestroyed -= OnBlockDestoyed;
    }

    private void OnBlockDestoyed(Vector2I position) {
        RemoveCollisionBlockAt(position.X, position.Y);
    }

    public void OnPlayerMovedCell(Vector2I newPosition, Vector2I oldPosition) {
        int radius = 3;
        int startX = Mathf.Max(0, newPosition.X - radius);
        int endX = Mathf.Min(_worldSize.X - 1, newPosition.X + radius);
        int startY = Mathf.Max(0, newPosition.Y - radius);
        int endY = Mathf.Min(_worldSize.Y - 1, newPosition.Y + radius);

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                if (_activeCollisionBlocks[x, y] == null && HasBlockEntity(x, y)) {
                    CreateCollisionBlock(x, y);
                }
            }
        }

        int oldStartX = Mathf.Max(0, oldPosition.X - radius);
        int oldEndX = Mathf.Min(_worldSize.X - 1, oldPosition.X + radius);
        int oldStartY = Mathf.Max(0, oldPosition.Y - radius);
        int oldEndY = Mathf.Min(_worldSize.Y - 1, oldPosition.Y + radius);

        for (int x = oldStartX; x <= oldEndX; x++) {
            for (int y = oldStartY; y <= oldEndY; y++) {
                if (x < startX || x > endX || y < startY || y > endY) {
                    RemoveCollisionBlockAt(x, y);
                }
            }
        }
    }

    private bool HasBlockEntity(int x, int y) {
        List<IEntity> entities = _entities[x, y];
        return entities != null && entities.OfType<BlockEntity>().Any();
    }

    private void CreateCollisionBlock(int x, int y) {
        StaticBody2D block = _collisionBlockScene.Instantiate<StaticBody2D>();
        block.Position = new Vector2(x * Game.BlockSize, y * Game.BlockSize);
        _world.AddChild(block);

        _activeCollisionBlocks[x, y] = block;
    }

    private void RemoveCollisionBlockAt(int x, int y) {
        if (x >= 0 && x < _activeCollisionBlocks.GetLength(0) &&
            y >= 0 && y < _activeCollisionBlocks.GetLength(1) &&
            _activeCollisionBlocks[x, y] != null) {
            _activeCollisionBlocks[x, y].QueueFree();
            _activeCollisionBlocks[x, y] = null;
        }
    }
}
