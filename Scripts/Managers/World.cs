using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    public Game Game { get; private set; }
    public Block[,] Blocks { get; private set; }
    public Vector2I WorldSize { get; private set; }
    public bool IsHost { get; private set; }
    public Vector2I DefaultSpawnPosition { get; private set; } = new(4, 14);
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public InputManager InputManager { get; private set; }

    // World sync constants
    private const int ChunkSize = 50;
    private Dictionary _localPlayerData;

    public event Action<Vector2I, string> BlockDestroyed; 
    public event Action<Vector2I> BlockCreated;

    public static World CreateAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.IsHost = true;
        world.WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        world.Blocks = new Block[world.WorldSize.X, world.WorldSize.Y];
        world.Game = game;
        world._localPlayerData = playerData;

        Array allWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            switch (dictionary["type"].ToString()) {
                case "block":
                    world.Blocks[x, y] = new Block() {
                        CurrentHealth = 1,
                        ResourcePath = dictionary["item"].AsGodotDictionary()["ResourcePath"].ToString(),
                    };
                    break;
                default:
                    throw new Exception(
                        $"[20250529.2332.1] Unknown world object type: {dictionary["type"].ToString()}");
            }
        }

        return world;
    }

    public static World CreateAsClient(Dictionary metadata, Dictionary playerData, Game game) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        world.Blocks = new Block[world.WorldSize.X, world.WorldSize.Y];
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
            PlayerManager.PlayerSpawnedOnServer += OnPlayerSpawnedOnServer;
            PlayerManager.SpawnHostPlayer(_localPlayerData);
            TreeExiting += () => {
                PlayerManager.PlayerSpawnedOnServer -= OnPlayerSpawnedOnServer;
            };
        } else {
            RpcId(1, nameof(RpcRequestWorldData));
        }
    }

    public override void _ExitTree() {
        Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
    }

    private void OnPlayerSpawnedOnServer(Player player) {
        player.ActionController.BuildAction.HostPlaceBlockAction 
            += OnHostPlaceBlockAction;
        player.ActionController.GatherAction.ServerGatherAction
            += OnServerGatherAction;
        player.TreeExiting += () => {
            player.ActionController.BuildAction.HostPlaceBlockAction 
                -= OnHostPlaceBlockAction;
            player.ActionController.GatherAction.ServerGatherAction
                -= OnServerGatherAction;
        };
    }

    private void OnHostPlaceBlockAction(Item item, Vector2I coords) {
        Rpc(nameof(RpcAllCreateBlock), item.ResourcePath, coords);
    }
    
    private void OnServerGatherAction(Vector2I coords, float damage) {
        Block block = Blocks[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllBlockDestroyed), coords, block.ResourcePath);
        }
    }
    
    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(string resourcePath, Vector2I coords) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        BlockCreated?.Invoke(coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllBlockDestroyed(Vector2I coords, string resourcePath) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, resourcePath);
    }

    public bool IsInBounds(Vector2I intVector) {
        return intVector.X >= 0
               && intVector.X < WorldSize.X
               && intVector.Y >= 0
               && intVector.Y < WorldSize.Y;
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
                Block block = Blocks[x, y];
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
                    Blocks[x, y] = new Block() {
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
    }

    #endregion
}