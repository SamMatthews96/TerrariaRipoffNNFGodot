using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    private Game _game;
    private Block[,] _blocks;

    private Dictionary _localPlayerData;
    private Player _localPlayer;

    [Export] private WorldCollision _worldCollision;
    private WorldRenderer _worldRenderer;

    // World sync constants
    private const int ChunkSize = 50;
    private readonly List<Dictionary> _bufferedChunks = new();

    private Vector2I _worldSize;

    public event Action WorldLoaded;
    public event Action<Vector2I> BlockDestroyed;
    public event Action<Vector2I> BlockCreated;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
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
        _worldCollision.InitAsHost(_blocks, _worldSize);
        _localPlayer = SpawnLocalPlayer();

        _worldCollision.IncrementObserverCounts(_localPlayer.Coords);
        _localPlayer.MovedCell += _worldCollision.MoveObserver;
        _worldRenderer = WorldRenderer.Create(_blocks, _worldSize, _localPlayer);
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
        Block block = _blocks[coords.X, coords.Y];
        if (block is null) return;

        block.CurrentHealth -= power;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcBlockDestroyed), coords);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcBlockDestroyed(Vector2I coords) {
        _blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords);
    }

    private void OnLocalPlayerBuildBlockAttempted(Player player, Item item, Vector2I coords) {
        RpcId(1, nameof(RpcOnPlayerBuildBlockAttempted),
            coords, item.ResourcePath);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcOnPlayerBuildBlockAttempted(Vector2I coords, string resourcePath) {
        if (_blocks[coords.X, coords.Y] != null) return;

        Rpc(nameof(RpcCreateBlock), coords, resourcePath);
    }

    [Rpc(CallLocal = true)]
    private void RpcCreateBlock(Vector2I coords, string resourcePath) {
        _blocks[coords.X, coords.Y] = new Block() {
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
        _localPlayer = SpawnLocalPlayer();
        _worldCollision.InitAsClient(_worldSize);
        _worldRenderer = WorldRenderer.Create(_blocks, _worldSize, _localPlayer);
        AddChild(_worldRenderer);

        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    #endregion
}