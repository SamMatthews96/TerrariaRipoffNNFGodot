using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BlockManager : Node2D {
    public Block?[,] Blocks { get; private set; }
    public Block?[,] Walls { get; private set; }

    [Export] private World _world;
    private PlayerManager _playerManager;
    private ItemIdBimap _itemIdBimap;
    private Vector2I _worldSize;

    public event Action SyncComplete;
    public event Action<Vector2I> BlockCreated;
    public event Action<Vector2I, ushort> BlockDestroyed;
    public event Action<Vector2I> WallCreated;
    public event Action<Vector2I, ushort> WallDestroyed;

    public override void _Ready() {
        _playerManager = _world.PlayerManager;
        _itemIdBimap = _world.ItemIdBimap;
        _worldSize = _world.WorldSize;

        Blocks = new Block?[_worldSize.X, _worldSize.Y];
        Walls = new Block?[_worldSize.X, _worldSize.Y];

        if (!_world.IsHost) return;

        Dictionary<ushort, Dictionary<int, Array>> savedBlocks =
            (Dictionary<ushort, Dictionary<int, Array>>)_world.WorldData["blocks"];
        foreach ((ushort itemId, Dictionary<int, Array> xDict) in savedBlocks) {
            foreach ((int x, Array yArray) in xDict) {
                foreach (int y in yArray) {
                    Blocks[x, y] = new Block {
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
                    Walls[x, y] = new Block {
                        CurrentHealth = 1,
                        ItemId = itemId
                    };
                }
            }
        }

        _playerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => { _playerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost; };
    }

    public void ClientGetWorldData() {
        RpcId(1, nameof(RpcRequestWorldData));
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
        Rpc(nameof(RpcAllCreateBlock), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(ushort itemId, Vector2I coords) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
        BlockCreated?.Invoke(coords);
    }

    private void OnHostPlacedWall(Item item, Vector2I coords) {
        ushort itemId = _itemIdBimap.GetId(item);
        Rpc(nameof(RpcAllCreateWall), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateWall(ushort itemId, Vector2I coords) {
        Walls[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
        WallCreated?.Invoke(coords);
    }

    private void OnHostGatheredBlock(Vector2I coords, float damage) {
        Block? res = Blocks[coords.X, coords.Y];
        if (res is not { } block) return;
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyBlock), coords, block.ItemId);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyBlock(Vector2I coords, ushort itemId) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, itemId);
    }

    private void OnHostGatheredWall(Vector2I coords, float damage) {
        Block? value = Walls[coords.X, coords.Y];
        if (value is not { } block) return;
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyWall), coords);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyWall(Vector2I coords) {
        Block? value = Walls[coords.X, coords.Y];
        if (value is not { } block) return;
        Walls[coords.X, coords.Y] = null;
        WallDestroyed?.Invoke(coords, block.ItemId);
    }

    #region World Synchronization

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcRequestWorldData() {
        int requestingPeerId = Multiplayer.GetRemoteSenderId();

        Dictionary data = new() {
            ["blocks"] = SerializeChunk(Blocks),
            ["walls"] = SerializeChunk(Walls)
        };
        
        RpcId(requestingPeerId, nameof(RpcProcessWorld),
            data);
    }

    private Dictionary<ushort, Dictionary> SerializeChunk(Block?[,] data) {
        Dictionary<ushort, Dictionary> groupedByItemId = new();

        for (int x = 0; x < _worldSize.X; x++) {
            for (int y = 0; y < _worldSize.Y; y++) {
                Block? value = data[x, y];
                if (value is not { } block) continue;
                
                if (!groupedByItemId.ContainsKey(block.ItemId)) {
                    groupedByItemId[block.ItemId] = new Dictionary();
                }

                if (!groupedByItemId[block.ItemId].ContainsKey($"{x}")) {
                    groupedByItemId[block.ItemId][$"{x}"] = new Array();
                }

                ((Array)groupedByItemId[block.ItemId][$"{x}"]).Add(y);
            }
        }

        return groupedByItemId;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcProcessWorld(Dictionary data) {
        Dictionary blocksByItemId = data["blocks"].AsGodotDictionary();
        Dictionary wallsByItemId = data["walls"].AsGodotDictionary();

        foreach (var kvp in blocksByItemId) {
            ushort itemId = (ushort)kvp.Key;
            Dictionary xDict = (Dictionary)kvp.Value;

            foreach (var xKvp in xDict) {
                int x = xKvp.Key.ToString().ToInt();
                Array yArray = (Array)xKvp.Value;

                foreach (int y in yArray) {
                    Blocks[x, y] = new Block {
                        CurrentHealth = 1,
                        ItemId = itemId
                    };
                }
            }
        }

        foreach (var kvp in wallsByItemId) {
            ushort itemId = (ushort)kvp.Key;
            Dictionary xDict = (Dictionary)kvp.Value;

            foreach (var xKvp in xDict) {
                int x = xKvp.Key.ToString().ToInt();
                Array yArray = (Array)xKvp.Value;

                foreach (int y in yArray) {
                    Walls[x, y] = new Block {
                        CurrentHealth = 1,
                        ItemId = itemId
                    };
                }
            }
        }

        SyncComplete?.Invoke();

    }

    #endregion
}