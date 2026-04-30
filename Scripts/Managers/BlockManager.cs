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
    public event Action<Vector2I, string> BlockDestroyed;
    public event Action<Vector2I> BlockCreated;
    public event Action<Vector2I, string> WallDestroyed;

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
                ResourcePath = dictionary["item"]
                    .AsGodotDictionary()["ResourcePath"].ToString(),
            };
        }

        Array savedWalls = _world.WorldData["walls"].AsGodotArray();
        foreach (Dictionary dictionary in savedWalls) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            Walls[x, y] = new Block {
                CurrentHealth = 1,
                ResourcePath = dictionary["item"]
                    .AsGodotDictionary()["ResourcePath"].ToString(),
            };
        }

        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => { _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost; };

        if (!_world.IsHost) {
            RpcId(1, nameof(RpcRequestWorldData));
        }
    }

    public void ClientGetWorldData() {
        RpcId(1, nameof(RpcRequestWorldData));
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionController.BuildAction.HostPlaceBlockAction
            += OnHostPlaceBlockAction;
        player.ActionController.BuildAction.HostPlaceWallAction +=
            OnHostPlaceWallAction;
        player.ActionController.GatherAction.HostGatherBlockAction +=
            OnHostGatherBlockAction;
        player.ActionController.GatherAction.HostGatherWallAction +=
            OnHostGatherWallAction;
        player.TreeExiting += () => {
            player.ActionController.BuildAction.HostPlaceBlockAction
                -= OnHostPlaceBlockAction;
            player.ActionController.BuildAction.HostPlaceWallAction -=
                OnHostPlaceWallAction;
            player.ActionController.GatherAction.HostGatherBlockAction -=
                OnHostGatherBlockAction;
            player.ActionController.GatherAction.HostGatherWallAction -=
                OnHostGatherWallAction;
        };
    }

    private void OnHostPlaceBlockAction(Item item, Vector2I coords) {
        Rpc(nameof(RpcAllCreateBlock), item.ResourcePath, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(string resourcePath, Vector2I coords) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        BlockCreated?.Invoke(coords);
    }

    private void OnHostPlaceWallAction(Item item, Vector2I coords) {
        Rpc(nameof(RpcAllCreateWall), item.ToDictionary(), coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateWall(Dictionary itemDict, Vector2I coords) {
        Walls[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = itemDict["ResourcePath"].ToString()
        };
    }

    private void OnHostGatherBlockAction(Vector2I coords, float damage) {
        Block block = Blocks[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyBlock), coords, block.ResourcePath);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyBlock(Vector2I coords, string resourcePath) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, resourcePath);
    }

    private void OnHostGatherWallAction(Vector2I coords, float damage) {
        Block block = Walls[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyWall), coords);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyWall(Vector2I coords) {
        string path = Walls[coords.X, coords.Y].ResourcePath;
        Walls[coords.X, coords.Y] = null;
        WallDestroyed?.Invoke(coords, path);
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

                Dictionary blockPacket = new() {
                    ["chunkX"] = chunkX,
                    ["chunkY"] = chunkY,
                    ["chunkIndex"] = chunkIndex,
                    ["totalChunks"] = totalChunks,
                    ["entities"] = blockData
                };

                Array wallData = SerializeChunk(chunkX, chunkY, Walls);
                Dictionary wallPacket = new() {
                    ["chunkX"] = chunkX,
                    ["chunkY"] = chunkY,
                    ["chunkIndex"] = chunkIndex,
                    ["totalChunks"] = totalChunks,
                    ["entities"] = wallData
                };

                RpcId(requestingPeerId, nameof(RpcProcessWorldChunk),
                    blockPacket, wallPacket);
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
                    ["type"] = "block",
                    ["x"] = x,
                    ["y"] = y,
                    ["health"] = block.CurrentHealth,
                    ["path"] = block.ResourcePath
                };
                chunkEntities.Add(entityData);
            }
        }

        return chunkEntities;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcProcessWorldChunk(Dictionary blockPacket, Dictionary wallPacket) {
        int chunkIndex = (int)blockPacket["chunkIndex"];
        int totalChunks = (int)blockPacket["totalChunks"];
        Array entities = blockPacket["entities"].AsGodotArray();
        Array wallEntities = wallPacket["entities"].AsGodotArray();

        // Deserialize entities into the world
        foreach (Dictionary entityData in entities) {
            int x = (int)entityData["x"];
            int y = (int)entityData["y"];

            Blocks[x, y] = new Block {
                CurrentHealth = (float)entityData["health"],
                ResourcePath = entityData["path"].ToString()
            };
        }

        foreach (Dictionary entityData in wallEntities) {
            int x = (int)entityData["x"];
            int y = (int)entityData["y"];

            Walls[x, y] = new Block {
                CurrentHealth = (float)entityData["health"],
                ResourcePath = entityData["path"].ToString()
            };
        }

        // Check if this is the last chunk
        if (chunkIndex == totalChunks - 1) {
            SyncComplete?.Invoke();
        }
    }

    #endregion
}