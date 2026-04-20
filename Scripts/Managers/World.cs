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

    private Dictionary _localPlayerData;

    [Export] public WorldCollision WorldCollision { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public InputManager InputManager { get; private set; }

    // World sync constants
    private const int ChunkSize = 50;
    private readonly List<Dictionary> _bufferedChunks = new();


    public event Action WorldLoaded;
    public event Action<Vector2I, string> BlockDestroyed; // coords, resourcePath
    public event Action<Vector2I> BlockCreated;

    public static World CreateAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        World world = Data.PackedScenes.World.Instantiate<World>();
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
        if (Multiplayer.IsServer()) {
            WorldLoaded?.Invoke();
            PlayerManager.SpawnHostPlayer(_localPlayerData);
            Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
        } else {
            RpcId(1, nameof(RpcRequestWorldData));
        }
        PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }
    
    public override void _ExitTree() {
        PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.ActionController.GatherAction.GatherAttempted += 
            OnLocalPlayerGatherAttempted;
        player.ActionController.BuildAction.BuildBlockActionAttempted += 
            OnLocalPlayerBuildBlockAttempted;
    }

    private void OnLocalPlayerGatherAttempted(Vector2I coords, Player player) {
        RpcId(1, nameof(RpcHostPlayerGatherAttempted),
            coords, player.PlayerEquipment.Pickaxe.Power);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostPlayerGatherAttempted(Vector2I coords, float power) {
        Block block = Blocks[coords.X, coords.Y];
        if (block is null) return;

        block.CurrentHealth -= power;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllBlockDestroyed), coords, block.ResourcePath);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllBlockDestroyed(Vector2I coords, string resourcePath) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, resourcePath);
    }

    private void OnLocalPlayerBuildBlockAttempted(Player player, Item item, Vector2I coords) {
        RpcId(1, nameof(RpcHostPlayerBuildBlockAttempted),
            coords, item.ResourcePath);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostPlayerBuildBlockAttempted(Vector2I coords, string resourcePath) {
        if (Blocks[coords.X, coords.Y] != null) return;

        Rpc(nameof(RpcAllCreateBlock), coords, resourcePath);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(Vector2I coords, string resourcePath) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        BlockCreated?.Invoke(coords);
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

        Dictionary metadata = new() {
            ["Width"] = WorldSize.X,
            ["Height"] = WorldSize.Y
        };
        RpcId(requestingPeerId, nameof(RpcReceiveWorldMetadata), metadata);

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

                RpcId(requestingPeerId, nameof(RpcReceiveWorldChunk), chunkPacket);
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
    private void RpcReceiveWorldMetadata(Dictionary metadata) {
        WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        Blocks = new Block[WorldSize.X, WorldSize.Y];

        // Process any buffered chunks that arrived before metadata
        if (_bufferedChunks.Count > 0) {
            foreach (Dictionary bufferedChunk in _bufferedChunks) {
                ProcessWorldChunk(bufferedChunk);
            }

            _bufferedChunks.Clear();
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcReceiveWorldChunk(Dictionary chunkPacket) {
        // If metadata hasn't arrived yet, buffer this chunk
        if (Blocks == null) {
            _bufferedChunks.Add(chunkPacket);
            return;
        }

        ProcessWorldChunk(chunkPacket);
    }

    private void ProcessWorldChunk(Dictionary chunkPacket) {
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
        WorldLoaded?.Invoke();
        PlayerManager.ClientSpawnPlayers(_localPlayerData);
        
        Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    #endregion
}