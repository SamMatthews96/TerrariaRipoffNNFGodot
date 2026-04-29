using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    public Game Game { get; private set; }
    public Vector2I WorldSize { get; private set; }
    public bool IsHost { get; private set; }
    public Vector2I DefaultSpawnPosition { get; private set; } = new(4, 14);
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public PropManager PropManager { get; private set; }
    [Export] public InputManager InputManager { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }

    // World sync constants
    private const int ChunkSize = 50;
    private Dictionary _localPlayerData;
    public Dictionary WorldData { get; private set; } 
    
    public static World CreateAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.IsHost = true;
        world.WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        world.Game = game;
        
        world._localPlayerData = playerData;
        world.WorldData = worldData;

        return world;
    }

    public static World CreateAsClient(Dictionary metadata, Dictionary playerData, Game game) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        world.Game = game;
        world._localPlayerData = playerData;
        return world;
    }
    
    private void OnExitGameClicked() {
        Visible = false;
        Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        QueueFree();
    }
    
    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
        if (IsHost) {
            PlayerManager.SpawnHostPlayer(_localPlayerData);
            _localPlayerData = null;
        } else {
            RpcId(1, nameof(RpcRequestWorldData));
        }
    }

    public override void _ExitTree() {
        Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
    }

    public bool IsInBounds(Vector2I coords) {
        return coords.X >= 0
               && coords.X < WorldSize.X
               && coords.Y >= 0
               && coords.Y < WorldSize.Y;
    }

    public bool IsCellFilled(Vector2I coords) {
        if (BlockManager.Blocks[coords.X,coords.Y] is not null) return true;
        return PropManager.PropCells.ContainsKey(coords);
    }

    public bool IsInOrthogonalRange(Vector2I a, Vector2I b, int range) {
        if (!IsInBounds(a) || !IsInBounds(b)) return false;
        if (Math.Abs(a.X - b.X) > range) return false;
        if (Math.Abs(a.Y - b.Y) > range) return false;
        return true;
    }

    #region World Synchronization

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcRequestWorldData() {
        int requestingPeerId = Multiplayer.GetRemoteSenderId();

        // Calculate chunks
        int chunksX = (int)Math.Ceiling((double)WorldSize.X / ChunkSize);
        int chunksY = (int)Math.Ceiling((double)WorldSize.Y / ChunkSize);
        int totalChunks = chunksX * chunksY;

        // Send chunks
        int chunkIndex = 0;
        for (int chunkX = 0; chunkX < chunksX; chunkX++) {
            for (int chunkY = 0; chunkY < chunksY; chunkY++) {
                Array chunkData = SerializeChunk(chunkX, chunkY);

                Dictionary chunkPacket = new() {
                    ["chunkX"] = chunkX,
                    ["chunkY"] = chunkY,
                    ["chunkIndex"] = chunkIndex,
                    ["totalChunks"] = totalChunks,
                    ["entities"] = chunkData
                };

                RpcId(requestingPeerId, nameof(RpcProcessWorldChunk), chunkPacket);
                chunkIndex++;
            }
        }
    }

    private Array SerializeChunk(int chunkX, int chunkY) {
        Array chunkEntities = new();

        int startX = chunkX * ChunkSize;
        int startY = chunkY * ChunkSize;
        int endX = Math.Min(startX + ChunkSize, WorldSize.X);
        int endY = Math.Min(startY + ChunkSize, WorldSize.Y);

        for (int x = startX; x < endX; x++) {
            for (int y = startY; y < endY; y++) {
                Block block = BlockManager.Blocks[x, y];
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
    private void RpcProcessWorldChunk(Dictionary chunkPacket) {
        int chunkIndex = (int)chunkPacket["chunkIndex"];
        int totalChunks = (int)chunkPacket["totalChunks"];
        Array entities = chunkPacket["entities"].AsGodotArray();

        // Deserialize entities into the world
        foreach (Dictionary entityData in entities) {
            int x = (int)entityData["x"];
            int y = (int)entityData["y"];

            switch (entityData["type"].ToString()) {
                case "block":
                    BlockManager.Blocks[x, y] = new Block() {
                        CurrentHealth = (float)entityData["health"],
                        ResourcePath = entityData["path"].ToString()
                    };
                    break;
                default:
                    throw new Exception($"[Client] Unknown entity type: {entityData["type"]}");
            }
        }

        // Check if this is the last chunk
        if (chunkIndex == totalChunks - 1) {
            OnWorldSyncComplete();
        }
    }

    private void OnWorldSyncComplete() {
        PlayerManager.SpawnPlayersOnClient(_localPlayerData);
        _localPlayerData = null;
    }

    #endregion
}