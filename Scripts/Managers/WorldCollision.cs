using System.Collections.Generic;
using Godot;
using GodotCollections = Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldCollision : Node2D {
    [Export] private World _world;
    [Export] private PackedScene _collisionBlockScene;
    [Export] private int _observerRadius = 3;

    private Block[,] _blocks;
    private readonly Dictionary<Vector2I, StaticBody2D> _activeCollisionBlocks = new();
    private readonly Dictionary<Vector2I, int> _observerCounts = new(); 
    private Vector2I _worldSize;

    public override void _Ready() {
        if (Multiplayer.IsServer()) {
            InitAsHost();
        } else {
            InitAsClient();
        }
    }

    private void InitAsHost() {
        _blocks = _world.Blocks;
        _worldSize = _world.WorldSize;
        
        _world.BlockDestroyed += OnBlockDestroyed;
        _world.BlockCreated += OnBlockCreated;
        _world.PickupManager.ServerPickupCreated += OnPickupCreated;
        _world.PickupManager.ServerPickupMoved += OnPickupMoved;
        _world.PickupManager.ServerPickupDestroyed += OnPickupDestroyed;
        _world.PlayerManager.PlayerSpawnedOnServer += OnPlayerSpawnedOnServer;
        TreeExiting += HostOnTreeExiting;
    }

    private void InitAsClient() {
        _worldSize = _world.WorldSize;
        RpcId(1, nameof(RpcRequestCollisionBlocks));
    }
    
    private void HostOnTreeExiting() {
        TreeExiting -= HostOnTreeExiting;
        _world.BlockDestroyed -= OnBlockDestroyed;
        _world.BlockCreated -= OnBlockCreated;
        _world.PickupManager.ServerPickupCreated -= OnPickupCreated;
        _world.PickupManager.ServerPickupMoved -= OnPickupMoved;
        _world.PickupManager.ServerPickupDestroyed -= OnPickupDestroyed;
        _world.PlayerManager.PlayerSpawnedOnServer -= OnPlayerSpawnedOnServer;
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
            AddChild(block);
            _activeCollisionBlocks[pos] = block;
        }
    }

    private void OnPlayerSpawnedOnServer(Player player) {
        IncrementObserverCounts(player.Coords);
        player.MovedCell += MoveObserver;
    }

    private void MoveObserver(Vector2I newPosition, Vector2I oldPosition) {
        IncrementObserverCounts(newPosition);
        DecrementObserverCounts(oldPosition);
    }

    private void IncrementObserverCounts(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
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
                    Rpc(nameof(RpcCreateCollisionBlock), cell);
                }
            }
        }
    }

    private void DecrementObserverCounts(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
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
                    Rpc(nameof(RpcRemoveCollisionBlock), x, y);
                }
            }
        }
    }

    private void OnBlockCreated(Vector2I position) {
        if (_observerCounts.TryGetValue(position, out int count) && count > 0) {
            Rpc(nameof(RpcCreateCollisionBlock), position);
        }
    }

    private void OnBlockDestroyed(Vector2I position, string _) {
        if (!Multiplayer.IsServer()) return;
        Rpc(nameof(RpcRemoveCollisionBlock), position.X, position.Y);
    }

    private void OnPickupCreated(Vector2I position) {
        if (!Multiplayer.IsServer()) return;
        IncrementObserverCounts(position);
    }

    private void OnPickupMoved(Vector2I newPosition, Vector2I oldPosition) {
        if (!Multiplayer.IsServer()) return;
        MoveObserver(newPosition, oldPosition);
    }
    
    private void OnPickupDestroyed(Vector2I position) {
        DecrementObserverCounts(position);
    }
    
    private bool HasBlockEntity(int x, int y) {
        return _blocks[x, y] != null;
    }

    [Rpc(CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcCreateCollisionBlock(Vector2I position) {
        StaticBody2D block = _collisionBlockScene.Instantiate<StaticBody2D>();
        block.Position = position * Game.BlockSize;
        AddChild(block);

        _activeCollisionBlocks[position] = block;
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