using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BlockManager : Node2D {
    public Block[,] Blocks { get; private set; }
    public Block[,] Walls { get; private set; }

    [Export] private World _world;

    public event Action SyncComplete;
    public event Action<Vector2I, UInt16> BlockDestroyed;
    public event Action<Vector2I> BlockCreated;
    public event Action<Vector2I, UInt16> WallDestroyed;

    private const int ChunkSize = 50;

    public override void _Ready() {
        Blocks = new Block[_world.WorldSize.X, _world.WorldSize.Y];
        Walls = new Block[_world.WorldSize.X, _world.WorldSize.Y];

        if (!_world.IsHost) return;

        Array savedBlocks = _world.WorldData["blocks"].AsGodotArray();
        foreach (Dictionary dictionary in savedBlocks) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            Blocks[x, y] = new Block {
                CurrentHealth = 1,
                ItemId = (UInt16)dictionary["item"]
            };
        }

        Array savedWalls = _world.WorldData["walls"].AsGodotArray();
        foreach (Dictionary dictionary in savedWalls) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            Walls[x, y] = new Block {
                CurrentHealth = 1,
                ItemId = (UInt16)dictionary["item"]
            };
        }

        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost;
        };
    }

    public void ClientGetWorldData() {
        RpcId(1, nameof(RpcRequestWorldData));
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionState.Build.HostPlacedBlock += OnHostPlacedBlock;
        player.ActionState.Build.HostPlacedWall += OnHostPlacedWall;
        player.ActionState.Gather.HostGatheredBlock += OnHostGatheredBlock;
        player.ActionState.Gather.HostGatheredWall += OnHostGatheredWall;
        player.TreeExiting += () => {
            player.ActionState.Build.HostPlacedBlock -= OnHostPlacedBlock;
            player.ActionState.Build.HostPlacedWall -= OnHostPlacedWall;
            player.ActionState.Gather.HostGatheredBlock -= OnHostGatheredBlock;
            player.ActionState.Gather.HostGatheredWall -= OnHostGatheredWall;
        };
    }

    private void OnHostPlacedBlock(Item item, Vector2I coords) {
        UInt16 itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcAllCreateBlock), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(UInt16 itemId, Vector2I coords) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
        BlockCreated?.Invoke(coords);
    }

    private void OnHostPlacedWall(Item item, Vector2I coords) {
        UInt16 itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcAllCreateWall), itemId, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateWall(UInt16 itemId, Vector2I coords) {
        Walls[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ItemId = itemId
        };
    }

    private void OnHostGatheredBlock(Vector2I coords, float damage) {
        Block block = Blocks[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyBlock), coords, block.ItemId);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyBlock(Vector2I coords, UInt16 itemId) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, itemId);
    }

    private void OnHostGatheredWall(Vector2I coords, float damage) {
        Block block = Walls[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyWall), coords);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyWall(Vector2I coords) {
        UInt16 itemId = Walls[coords.X, coords.Y].ItemId;
        Walls[coords.X, coords.Y] = null;
        WallDestroyed?.Invoke(coords, itemId);
    }

    #region World Synchronization

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcRequestWorldData() {
        int requestingPeerId = Multiplayer.GetRemoteSenderId();

        // Calculate chunks
        int chunksX = (int)Math.Ceiling((double)_world.WorldSize.X / ChunkSize);
        int chunksY = (int)Math.Ceiling((double)_world.WorldSize.Y / ChunkSize);
        int totalChunks = chunksX * chunksY;

        // Send chunks
        int chunkIndex = 0;
        for (int chunkX = 0; chunkX < chunksX; chunkX++) {
            for (int chunkY = 0; chunkY < chunksY; chunkY++) {
                Array blockData = SerializeChunk(chunkX, chunkY, Blocks);
                Array wallData = SerializeChunk(chunkX, chunkY, Walls);
                
                Dictionary chunkPacket = new() {
                    ["chunkX"] = chunkX,
                    ["chunkY"] = chunkY,
                    ["chunkIndex"] = chunkIndex,
                    ["totalChunks"] = totalChunks,
                    ["blocks"] = blockData,
                    ["walls"] = wallData
                };

                RpcId(requestingPeerId, nameof(RpcProcessWorldChunk),
                    chunkPacket);
                chunkIndex++;
            }
        }
    }

    private Array SerializeChunk(int chunkX, int chunkY, Block[,] data) {
        Array chunkEntities = new();

        int startX = chunkX * ChunkSize;
        int startY = chunkY * ChunkSize;
        int endX = Math.Min(startX + ChunkSize, _world.WorldSize.X);
        int endY = Math.Min(startY + ChunkSize, _world.WorldSize.Y);

        for (int x = startX; x < endX; x++) {
            for (int y = startY; y < endY; y++) {
                Block block = data[x, y];
                if (block is null) continue;
                Dictionary entityData = new() {
                    ["x"] = x,
                    ["y"] = y,
                    ["health"] = block.CurrentHealth,
                    ["itemId"] = block.ItemId
                };
                chunkEntities.Add(entityData);
            }
        }

        return chunkEntities;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcProcessWorldChunk(Dictionary chunkPacket) {
        int chunkIndex = (int)chunkPacket["chunkIndex"];
        int totalChunks = (int)chunkPacket["totalChunks"];
        Array blockEntities = chunkPacket["blocks"].AsGodotArray();
        Array wallEntities = chunkPacket["walls"].AsGodotArray();

        foreach (Dictionary entityData in blockEntities) {
            int x = (int)entityData["x"];
            int y = (int)entityData["y"];

            Blocks[x, y] = new Block {
                CurrentHealth = (float)entityData["health"],
                ItemId = (UInt16)entityData["itemId"]
            };
        }

        foreach (Dictionary entityData in wallEntities) {
            int x = (int)entityData["x"];
            int y = (int)entityData["y"];

            Walls[x, y] = new Block {
                CurrentHealth = (float)entityData["health"],
                ItemId = (UInt16)entityData["itemId"]
            };
        }

        // Check if this is the last chunk
        if (chunkIndex == totalChunks - 1) {
            SyncComplete?.Invoke();
        }
    }

    #endregion
}