using System.Collections.Generic;
using Godot;
using GodotCollections = Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _collisionBlockScene;
    [Export] private int _observerRadius = 3;

    private Block[,] _blocks;
    private Dictionary<Vector2I, StaticBody2D> _activeCollisionBlocks;
    private int[,] _observerCounts; // Track how many observers are near each cell
    private Vector2I _worldSize;

    public void InitAsHost(Block[,] blocks, Vector2I worldSize) {
        _blocks = blocks;
        _worldSize = worldSize;
        _activeCollisionBlocks = new Dictionary<Vector2I, StaticBody2D>();
        _observerCounts = new int[worldSize.X, worldSize.Y];
    }
    
    public void InitAsClient(Vector2I worldSize) {
        _worldSize = worldSize;
        _activeCollisionBlocks = new Dictionary<Vector2I, StaticBody2D>();
        RpcId(1, nameof(RpcRequestCollisionBlocks));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRequestCollisionBlocks() {
        int senderId = Multiplayer.GetRemoteSenderId();
        GodotCollections.Array<Vector2I> positions = new(_activeCollisionBlocks.Keys);
        RpcId(senderId, nameof(RpcReceiveCollisionBlocks), positions);
    }

    [Rpc(CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcReceiveCollisionBlocks(GodotCollections.Array<Vector2I> positions) {
        foreach (Vector2I pos in positions) {
            StaticBody2D block = _collisionBlockScene.Instantiate<StaticBody2D>();
            block.Position = new Vector2(pos.X * Game.BlockSize, pos.Y * Game.BlockSize);
            _world.AddChild(block);
            _activeCollisionBlocks[pos] = block;
        }
    }

    public override void _Ready() {
        if (!Multiplayer.IsServer()) return; 
        _world.BlockDestroyed += OnBlockDestroyed;
        _world.BlockCreated += OnBlockCreated;
    }

    public override void _ExitTree() {
        if (!Multiplayer.IsServer()) return; 
        _world.BlockDestroyed -= OnBlockDestroyed;
        _world.BlockCreated -= OnBlockCreated;
    }

    public void MoveObserver(Vector2I newPosition, Vector2I oldPosition) {
        IncrementObserverCounts(newPosition);
        DecrementObserverCounts(oldPosition);
    }

    public void IncrementObserverCounts(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
        int startX = Mathf.Max(0, position.X - _observerRadius);
        int endX = Mathf.Min(_worldSize.X - 1, position.X + _observerRadius);
        int startY = Mathf.Max(0, position.Y - _observerRadius);
        int endY = Mathf.Min(_worldSize.Y - 1, position.Y + _observerRadius);

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                _observerCounts[x, y]++;
                if (_observerCounts[x, y] == 1 && HasBlockEntity(x, y)) {
                    Rpc(nameof(RpcCreateCollisionBlock), x, y);
                }
            }
        }
    }
    
    public void DecrementObserverCounts(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
        int startX = Mathf.Max(0, position.X - _observerRadius);
        int endX = Mathf.Min(_worldSize.X - 1, position.X + _observerRadius);
        int startY = Mathf.Max(0, position.Y - _observerRadius);
        int endY = Mathf.Min(_worldSize.Y - 1, position.Y + _observerRadius);

        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                _observerCounts[x, y]--;
                if (_observerCounts[x, y] == 0 && HasBlockEntity(x, y)) {
                    Rpc(nameof(RpcRemoveCollisionBlock), x, y);
                }
            }
        }
    }

    private void OnBlockDestroyed(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
        Rpc(nameof(RpcRemoveCollisionBlock), position.X, position.Y);
    }

    private void OnBlockCreated(Vector2I position) {
        if (_observerCounts[position.X, position.Y] > 0) {
            Rpc(nameof(RpcCreateCollisionBlock), position.X, position.Y);
        }
    }
    
    private bool HasBlockEntity(int x, int y) {
        return _blocks[x, y] != null;
    }

    [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcCreateCollisionBlock(int x, int y) {
        StaticBody2D block = _collisionBlockScene.Instantiate<StaticBody2D>();
        block.Position = new Vector2(x * Game.BlockSize, y * Game.BlockSize);
        _world.AddChild(block);

        _activeCollisionBlocks[new Vector2I(x, y)] = block;
    }

    [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRemoveCollisionBlock(int x, int y) {
        Vector2I pos = new(x, y);
        if (_activeCollisionBlocks.TryGetValue(pos, out StaticBody2D block)) {
            block.QueueFree();
            _activeCollisionBlocks.Remove(pos);
        }
    }
}
