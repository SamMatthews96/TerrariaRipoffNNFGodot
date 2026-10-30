using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BlockManager : Node2D {
    private Block?[,] _blocks;
    private Block?[,] _walls;

    [Export] private World _world;
    private PlayerManager _playerManager;
    private ItemIdBimap _itemIdBimap;
    private Vector2I _worldSize;

    public event Action<Vector2I> BlockCreated;
    public event Action<Vector2I, ushort> BlockDestroyed;
    public event Action<Vector2I> WallCreated;
    public event Action<Vector2I, ushort> WallDestroyed;
    
    #region public utils
    public Block? GetBlock(int x, int y) => _blocks[x, y];
    public Block? GetBlock(Vector2I coords) => _blocks[coords.X, coords.Y];
    public Block? GetWall(int x, int y) => _walls[x, y];
    public Block? GetWall(Vector2I coords) => _walls[coords.X, coords.Y];

    public Dictionary<string, Dictionary> SerializeBlocks() {
        return SerializeData(_blocks);
    }
    
    public Dictionary<string, Dictionary> SerializeWalls() {
        return SerializeData(_walls);
    }
    
    private Dictionary<string, Dictionary> SerializeData(Block?[,] data) {
        Dictionary<string, Dictionary> groupedByItemId = new();
        int itemCount = _itemIdBimap.GetItemCount();
        for (int i = 0; i < itemCount; i++) {
            string idStr = i.ToString();
            groupedByItemId[idStr] = new Dictionary();
        }

        for (int x = 0; x < _worldSize.X; x++) {
            string xStr = x.ToString();

            // Pre-create arrays for this column and cache references
            Array[] columnArrays = new Array[itemCount];
            for (int i = 0; i < itemCount; i++) {
                string idStr = i.ToString();
                Array arr = new();
                groupedByItemId[idStr][xStr] = arr;
                columnArrays[i] = arr;
            }

            for (int y = 0; y < _worldSize.Y; y++) {
                Block? value = data[x, y];
                if (value is not { } block) continue;

                columnArrays[block.ItemId].Add(y);
            }
        }

        return groupedByItemId;
    }

    #endregion
    
    public override void _Ready() {
        _playerManager = _world.PlayerManager;
        _itemIdBimap = _world.ItemIdBimap;
        _worldSize = _world.WorldSize;

        _blocks = new Block?[_worldSize.X, _worldSize.Y];
        _walls = new Block?[_worldSize.X, _worldSize.Y];

        Dictionary<ushort, Dictionary<int, Array>> savedBlocks =
            (Dictionary<ushort, Dictionary<int, Array>>)_world.WorldData["blocks"];
        foreach ((ushort itemId, Dictionary<int, Array> xDict) in savedBlocks) {
            foreach ((int x, Array yArray) in xDict) {
                foreach (int y in yArray) {
                    _blocks[x, y] = new Block {
                        CurrentHealth = 1,
                        ItemId = itemId
                    };
                }
            }
        }

        Dictionary<ushort, Dictionary<int, Array>> savedWalls =
            (Dictionary<ushort, Dictionary<int, Array>>)_world.WorldData["walls"];
        foreach ((ushort itemId, Dictionary<int, Array> xDict) in savedWalls) {
            foreach ((int x, Array yArray) in xDict) {
                foreach (int y in yArray) {
                    _walls[x, y] = new Block {
                        CurrentHealth = 1,
                        ItemId = itemId
                    };
                }
            }
        }

        _playerManager.HostPlayerSpawned += OnPlayerSpawnedOnHost;
        TreeExiting += () => { _playerManager.HostPlayerSpawned -= OnPlayerSpawnedOnHost; };
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.BlockPlaced += OnHostPlacedBlock;
        player.WallPlaced += OnHostPlacedWall;
        player.BlockGathered += OnHostGatheredBlock;
        player.WallGathered += OnHostGatheredWall;
        player.TreeExiting += () => {
            player.BlockPlaced -= OnHostPlacedBlock;
            player.WallPlaced -= OnHostPlacedWall;
            player.BlockGathered -= OnHostGatheredBlock;
            player.WallGathered -= OnHostGatheredWall;
        };
    }

    private void OnHostPlacedBlock(Item item, Vector2I coords) {
        ushort itemId = _itemIdBimap.GetId(item);
        this.RpcAll(nameof(RpcAllCreateBlock), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(ushort itemId, Vector2I coords) {
        _blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
        BlockCreated?.Invoke(coords);
    }

    private void OnHostPlacedWall(Item item, Vector2I coords) {
        ushort itemId = _itemIdBimap.GetId(item);
        this.RpcAll(nameof(RpcAllCreateWall), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateWall(ushort itemId, Vector2I coords) {
        _walls[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
        WallCreated?.Invoke(coords);
    }

    private void OnHostGatheredBlock(Vector2I coords, float damage) {
        Block? res = _blocks[coords.X, coords.Y];
        if (res is not { } block) return;
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            this.RpcAll(nameof(RpcAllDestroyBlock), coords, block.ItemId);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyBlock(Vector2I coords, ushort itemId) {
        _blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, itemId);
    }

    private void OnHostGatheredWall(Vector2I coords, float damage) {
        Block? value = _walls[coords.X, coords.Y];
        if (value is not { } block) return;
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            this.RpcAll(nameof(RpcAllDestroyWall), coords);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyWall(Vector2I coords) {
        Block? value = _walls[coords.X, coords.Y];
        if (value is not { } block) return;
        _walls[coords.X, coords.Y] = null;
        WallDestroyed?.Invoke(coords, block.ItemId);
    }
}