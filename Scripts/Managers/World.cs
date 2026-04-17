using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    private Game _game;
    private List<IEntity>[,] _entities;

    private Dictionary _localPlayerData;
    private Player _localPlayer;

    [Export] private WorldCollision _worldCollision;
    [Export] private WorldRenderer _worldRenderer;

    // World sync constants
    private const int ChunkSize = 50;
    private bool _isReceivingWorldData;
    private readonly List<Dictionary> _bufferedChunks = new();

    public Vector2I WorldSize { get; private set; }

    public event Action WorldLoaded;
    public event Action<Vector2I> BlockDestroyed;
    public event Action<Vector2I> BlockCreated;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;
        WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        _entities = new List<IEntity>[WorldSize.X, WorldSize.Y];
        for (int x = 0; x < WorldSize.X; x++) {
            for (int y = 0; y < WorldSize.Y; y++) {
                _entities[x, y] = new List<IEntity>();
            }
        }

        Array allWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            IEntity entity;
            switch (dictionary["type"].ToString()) {
                case "block":
                    entity = new BlockEntity() {
                        CellCoordinates = new Vector2I(x, y),
                        CurrentHealth = 1,
                        ResourcePath = dictionary["item"].AsGodotDictionary()["ResourcePath"].ToString(),
                    };
                    break;
                default:
                    throw new Exception(
                        $"[20250529.2332.1] Unknown world object type: {dictionary["type"].ToString()}");
            }

            _entities[x, y].Add(entity);
        }

        WorldLoaded?.Invoke();
        _worldCollision.InitAsHost(_entities, WorldSize);
        _localPlayer = SpawnLocalPlayer();

        _worldCollision.IncrementObserverCounts(_localPlayer.Coords);
        _localPlayer.MovedCell += _worldCollision.MoveObserver;
        _worldRenderer = WorldRenderer.Create(_entities, WorldSize, _localPlayer);
        AddChild(_worldRenderer);
        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    public override void _ExitTree() {
        if (_localPlayer != null) {
            _localPlayer.ActionController.GatherAction.GatherAttempted -=
                OnLocalPlayerGatherAttempted;
        }
    }

    private void OnExitGameClicked() {
        Visible = false;
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        QueueFree();
    }

    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;

        _isReceivingWorldData = true;
        RpcId(1, nameof(RpcRequestWorldData));
    }

    private Player SpawnLocalPlayer() {
        Player player = Player.Create(_game.PeerId, new Vector2I(4, 14));
        player.InitAsLocal(_game, _localPlayerData);
        AddChild(player, true);
        player.ActionController.GatherAction.GatherAttempted += OnLocalPlayerGatherAttempted;
        player.ActionController.BuildAction.BuildBlockActionAttempted += OnLocalPlayerBuildBlockAttempted;
        
        Rpc(nameof(RpcOnNewPlayerJoining));
        return player;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcOnNewPlayerJoining() {
        int newPeerId = Multiplayer.GetRemoteSenderId();
        Player player = Player.Create(newPeerId, new Vector2I(4, 14));
        AddChild(player, true);

        if (Multiplayer.IsServer()) {
            _worldCollision.IncrementObserverCounts(player.Coords);
            player.MovedCell += _worldCollision.MoveObserver;
        }

        _localPlayer.AddPeerToSynchronizer(newPeerId);

        RpcId(newPeerId, nameof(SpawnRemoteExistingPlayer), _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SpawnRemoteExistingPlayer(int peerId) {
        Player player = Player.Create(peerId, new Vector2I(4, 14));
        AddChild(player, true);
    }

    private void OnLocalPlayerGatherAttempted(Vector2I coords, Player player) {
        RpcId(1, nameof(RpcOnPlayerGatherAttempted),
            coords, player.PlayerEquipment.Pickaxe.Power);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcOnPlayerGatherAttempted(Vector2I coords, float power) {
        List<IEntity> cellEntities = _entities[coords.X, coords.Y];

        for (int i = 0; i < cellEntities.Count; i++) {
            IEntity entity = cellEntities[i];
            if (entity is BlockEntity blockEntity) {
                blockEntity.CurrentHealth -= power;
                if (blockEntity.CurrentHealth <= 0) {
                    Rpc(nameof(RpcBlockDestroyed), coords, i);
                }
                break;
            }
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcBlockDestroyed(Vector2I coords, int i) {
        _entities[coords.X, coords.Y].RemoveAt(i);
        BlockDestroyed?.Invoke(coords);
    }
    
    private void OnLocalPlayerBuildBlockAttempted(Player player, Item item, Vector2I coords) {
        RpcId(1, nameof(RpcOnPlayerBuildBlockAttempted),
            coords, item.ResourcePath);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcOnPlayerBuildBlockAttempted(Vector2I coords, string resourcePath) {
        List<IEntity> cellEntities = _entities[coords.X, coords.Y];
        if (cellEntities.OfType<BlockEntity>().Any()) return;

        Rpc(nameof(RpcCreateBlock), coords, resourcePath);
    }

    [Rpc(CallLocal = true)]
    private void RpcCreateBlock(Vector2I coords, string resourcePath) {
        BlockEntity block = new() {
            CellCoordinates = coords,
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        _entities[coords.X, coords.Y].Add(block);
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
                foreach (IEntity entity in _entities[x, y]) {
                    if (entity is BlockEntity blockEntity) {
                        Dictionary entityData = new() {
                            ["type"] = "block",
                            ["x"] = x,
                            ["y"] = y,
                            ["health"] = blockEntity.CurrentHealth,
                            ["path"] = blockEntity.ResourcePath
                        };
                        chunkEntities.Add(entityData);
                    }
                }
            }
        }

        return chunkEntities;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcReceiveWorldMetadata(Dictionary metadata) {
        WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        _entities = new List<IEntity>[WorldSize.X, WorldSize.Y];

        for (int x = 0; x < WorldSize.X; x++) {
            for (int y = 0; y < WorldSize.Y; y++) {
                _entities[x, y] = new List<IEntity>();
            }
        }

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
        if (_entities == null) {
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

            IEntity entity;
            switch (entityData["type"].ToString()) {
                case "block":
                    entity = new BlockEntity() {
                        CellCoordinates = new Vector2I(x, y),
                        CurrentHealth = (float)entityData["health"],
                        ResourcePath = entityData["path"].ToString()
                    };
                    break;
                default:
                    throw new Exception($"[Client] Unknown entity type: {entityData["type"]}");
            }

            _entities[x, y].Add(entity);
        }

        // Check if this is the last chunk
        if (chunkIndex == totalChunks - 1) {
            OnWorldSyncComplete();
        }
    }

    private void OnWorldSyncComplete() {
        _isReceivingWorldData = false;

        WorldLoaded?.Invoke();
        _localPlayer = SpawnLocalPlayer();
        _worldCollision.InitAsClient(WorldSize);
        _worldRenderer = WorldRenderer.Create(_entities, WorldSize, _localPlayer);
        AddChild(_worldRenderer);

        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }
    
    #endregion
}