using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public const int BLOCK_SIZE = 32;
    /*
     * This class manages the state of the saved blocks
     * it is responsible for creating the saved blocks
     * and updating the saved blocks
     *
     * It might later be responsible for other world objects, such as NPCs, chests, etc
     */

    public static WorldManager Instance { get; private set; }

    private World _world;
    private PlayerInfo _playerInfo;

    [Export] private LocalObjectSpawnManager localObjectSpawnManager;
    [Export] private PlayerManager playerManager;
    
    [Signal]
    public delegate void ServerDeletedActiveBlockEventHandler(BlockType blockType, int xPosition, int yPosition);

    [Signal]
    public delegate void InitializedEventHandler();

    public override void _Ready() {
        Instance = this;
    }

    #region Getters

    public GridPosition GetPlayerSpawnPosition() {
        return _world.DefaultSpawnPosition;
    }

    public Vector2 GetWorldPositionFromCellCoordinates(int xCoordinate, int yCoordinate) {
        return new Vector2(xCoordinate * BLOCK_SIZE, yCoordinate * BLOCK_SIZE);
    }

    public List<SavedBlock> GetSavedBlocksInRegion(int xCoordinate, int yCoordinate) {
        (int xStart, int xEnd, int yStart, int yEnd) = _world.GetRegionBoundary(xCoordinate, yCoordinate);

        List<SavedBlock> savedBlocks = new();
        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                if (_world.SavedBlocks[x, y] is null) continue;
                savedBlocks.Add(_world.SavedBlocks[x, y]);
            }
        }

        return savedBlocks;
    }

    #endregion

    #region WorldCreation

    private void OnWorldLoaded(World world, PlayerInfo playerInfo) {
        _world = world;
        for (int x = 0; x < _world.Width; x++) {
            for (int y = 0; y < _world.Height; y++) {
                SavedBlock savedBlock = _world.SavedBlocks[x, y];
                if (savedBlock is null) continue;
                savedBlock.Destroyed += OnSavedBlockDestroyed;
            }
        }

        localObjectSpawnManager.Initialize(world.Width, world.Height);
        playerManager.Initialize(playerInfo);

        EmitSignal(SignalName.Initialized);
    }

    private void OnConnectedToServer(PlayerInfo playerInfo) {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerInitialiseWorldForNewPlayer),
            peerId, playerInfo.UniqueName);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServerInitialiseWorldForNewPlayer(int peerId, string playerUniqueName) {
        _world.PlayerPositions.TryAdd(playerUniqueName, _world.DefaultSpawnPosition);
        int xSpawnCoordinate = _world.PlayerPositions[playerUniqueName].X;
        int ySpawnCoordinate = _world.PlayerPositions[playerUniqueName].Y;

        World initialWorld = _world.GetInitialWorldAroundSpawn(xSpawnCoordinate, ySpawnCoordinate);
        Dictionary initialWorldSerialized = initialWorld.Serialize();

        RpcId(peerId, nameof(PeerCreateWorld), initialWorldSerialized);
    }

    [Rpc]
    private void PeerCreateWorld(Dictionary initialWorldSerialized) {
        _world = World.FromDict(initialWorldSerialized);
    }

    #endregion

    #region Block Changes

    private void OnActiveBlockTakenDamage(int xPosition, int yPosition, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(DamageSavedBlock),
            xPosition, yPosition, damageAmount);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        SavedBlock savedBlock = _world.SavedBlocks[xPosition, yPosition];
        savedBlock?.TakeDamage(damageAmount);
    }

    private void OnSavedBlockDestroyed(int xPosition, int yPosition) {
        BlockType blockType = _world.SavedBlocks[xPosition, yPosition].BlockType;
        _world.SavedBlocks[xPosition, yPosition] = null;
        EmitSignal(SignalName.ServerDeletedActiveBlock, blockType, xPosition, yPosition);
    }

    private void OnPlayerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerAttemptBuildBlock),
            xPosition, yPosition, blockResourcePath);
    }


    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
        BlockType blockType = BlockType.Deserialize(blockResourcePath);
        if (!_world.AreCoordsInBounds(xPosition, yPosition)) return;
        if (_world.SavedBlocks[xPosition, yPosition] is not null) return;
        SavedBlock savedBlock = SavedBlock.Builder.New(blockType, xPosition, yPosition).Build();
        _world.SavedBlocks[xPosition, yPosition] = savedBlock;
    }

    #endregion block changes
}