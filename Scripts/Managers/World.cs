using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    public Game Game { get; private set; }
    private Block[,] _blocks;

    private Dictionary _localPlayerData;
    // private Player _localPlayer;

    [Export] public WorldCollision WorldCollision { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    
    private WorldRenderer _worldRenderer;

    // World sync constants
    private const int ChunkSize = 50;
    private readonly List<Dictionary> _bufferedChunks = new();

    private Vector2I _worldSize;

    public event Action WorldLoaded;
    public event Action<Vector2I, string> BlockDestroyed; // coords, resourcePath
    public event Action<Vector2I> BlockCreated;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (Game is not null) throw new Exception("[20250529.2332.1] Game already set");
        Game = game;
        _localPlayerData = playerData;
        _worldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        _blocks = new Block[_worldSize.X, _worldSize.Y];

        Array allWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            switch (dictionary["type"].ToString()) {
                case "block":
                    _blocks[x, y] = new Block() {
                        CurrentHealth = 1,
                        ResourcePath = dictionary["item"].AsGodotDictionary()["ResourcePath"].ToString(),
                    };
                    break;
                default:
                    throw new Exception(
                        $"[20250529.2332.1] Unknown world object type: {dictionary["type"].ToString()}");
            }
        }

        WorldLoaded?.Invoke();
        WorldCollision.InitAsHost(_blocks, _worldSize);
        
        PlayerManager.SpawnHostPlayer(playerData);

        // _worldRenderer = WorldRenderer.Create(_blocks, _worldSize, _localPlayer);
        // AddChild(_worldRenderer);

        Game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }


    private void OnExitGameClicked() {
        Visible = false;
        Game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        QueueFree();
    }

    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (Game is not null) throw new Exception("[20250529.2332.1] Game already set");
        Game = game;
        _localPlayerData = playerData;

        RpcId(1, nameof(RpcRequestWorldData));
    }
    
    public override void _Ready() {
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
        Block block = _blocks[coords.X, coords.Y];
        if (block is null) return;

        block.CurrentHealth -= power;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllBlockDestroyed), coords, block.ResourcePath);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllBlockDestroyed(Vector2I coords, string resourcePath) {
        _blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, resourcePath);
    }

    private void OnLocalPlayerBuildBlockAttempted(Player player, Item item, Vector2I coords) {
        RpcId(1, nameof(RpcHostPlayerBuildBlockAttempted),
            coords, item.ResourcePath);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostPlayerBuildBlockAttempted(Vector2I coords, string resourcePath) {
        if (_blocks[coords.X, coords.Y] != null) return;

        Rpc(nameof(RpcAllCreateBlock), coords, resourcePath);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(Vector2I coords, string resourcePath) {
        _blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        BlockCreated?.Invoke(coords);
    }

    public bool IsInBounds(Vector2I intVector) {
        return intVector.X >= 0
               && intVector.X < _worldSize.X
               && intVector.Y >= 0
               && intVector.Y < _worldSize.Y;
    }

    #region World Synchronization

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcRequestWorldData() {
        int requestingPeerId = Multiplayer.GetRemoteSenderId();

        Dictionary metadata = new() {
            ["Width"] = _worldSize.X,
            ["Height"] = _worldSize.Y
        };
        RpcId(requestingPeerId, nameof(RpcReceiveWorldMetadata), metadata);

        // Calculate chunks
        int chunksX = (int)Math.Ceiling((double)_worldSize.X / ChunkSize);
        int chunksY = (int)Math.Ceiling((double)_worldSize.Y / ChunkSize);
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
        int endX = Math.Min(startX + ChunkSize, _worldSize.X);
        int endY = Math.Min(startY + ChunkSize, _worldSize.Y);

        for (int x = startX; x < endX; x++) {
            for (int y = startY; y < endY; y++) {
                Block block = _blocks[x, y];
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
        _worldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        _blocks = new Block[_worldSize.X, _worldSize.Y];

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
        if (_blocks == null) {
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
                    _blocks[x, y] = new Block() {
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
        
        WorldCollision.InitAsClient(_worldSize);
        // _worldRenderer = WorldRenderer.Create(_blocks, _worldSize, _localPlayer);
        // AddChild(_worldRenderer);

        Game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    #endregion
}