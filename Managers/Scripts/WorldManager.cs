using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;
using GodotDictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public const int BLOCK_SIZE = 32;
    /*
     * This class manages the state of the saved blocks
     * it is responsible for creating the saved blocks
     * and updating the saved blocks
     *
     * It might later be responsible for other worldDictionary objects, such as NPCs, chests, etc
     */

    public static WorldManager Instance { get; private set; }

    private PlayerInfo _playerInfo;
    private int _width;
    private int _height;
    private SavedBlock[,] _savedBlocks;
    private Dictionary<string, GridPosition> _playerPositions = new();

    public GridPosition DefaultSpawnPosition { get; private set; }

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
        return DefaultSpawnPosition;
    }

    public Vector2 GetWorldPositionFromCellCoordinates(int xCoordinate, int yCoordinate) {
        return new Vector2(xCoordinate * BLOCK_SIZE, yCoordinate * BLOCK_SIZE);
    }

    public List<SavedBlock> GetSavedBlocksInRegion(int xCoordinate, int yCoordinate) {
        (int xStart, int xEnd, int yStart, int yEnd) = GetRegionBoundary(xCoordinate, yCoordinate);

        List<SavedBlock> savedBlocks = new();
        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                if (_savedBlocks[x, y] is null) continue;
                savedBlocks.Add(_savedBlocks[x, y]);
            }
        }

        return savedBlocks;
    }

    private (int xStart, int xEnd, int yStart, int yEnd) GetRegionBoundary(int xCoord, int yCoord) {
        int xStart = Math.Max(0, xCoord - LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int xEnd = Math.Min(_width - 1, xCoord + LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int yStart = Math.Max(0, yCoord - LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        int yEnd = Math.Min(_height - 1, yCoord + LocalObjectSpawnManager.BLOCK_RENDER_DISTANCE);
        return (xStart, xEnd, yStart, yEnd);
    }

    private bool AreCoordsInBounds(int xPosition, int yPosition) {
        if (xPosition < 0) return false;
        if (yPosition < 0) return false;
        if (xPosition >= _width) return false;
        if (yPosition >= _height) return false;
        return true;
    }

    #endregion

    #region WorldCreation

    private void OnMainMenuSceneWorldLoaded(GodotDictionary worldDictionary, PlayerInfo playerInfo) {
        playerManager.Initialize(playerInfo);
        Initialize(worldDictionary);
    }

    private void Initialize(GodotDictionary worldDict) {
        _width = worldDict["Width"].ToString().ToInt();
        _height = worldDict["Height"].ToString().ToInt();
        Array savedBlockArray = worldDict["SavedBlocks"].AsGodotArray();
        _savedBlocks = new SavedBlock[_width, _height];
        foreach (GodotDictionary savedBlockDictionary in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }

        _playerPositions = new Dictionary<string, GridPosition>();
        Array defaultSpawnPosition = worldDict["DefaultSpawnPosition"].AsGodotArray();
        DefaultSpawnPosition = new GridPosition(
            defaultSpawnPosition[0].ToString().ToInt(),
            defaultSpawnPosition[1].ToString().ToInt());
        localObjectSpawnManager.Initialize(_width, _height);
        EmitSignal(SignalName.Initialized);
    }

    private void OnConnectedToServer(PlayerInfo playerInfo) {
        int peerId = Multiplayer.GetUniqueId();
        playerManager.Initialize(playerInfo);
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerInitialiseWorldForNewPlayer),
            peerId, playerInfo.UniqueName);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServerInitialiseWorldForNewPlayer(int peerId, string playerUniqueName) {
        _playerPositions.TryAdd(playerUniqueName, DefaultSpawnPosition);
        int xSpawnCoordinate = _playerPositions[playerUniqueName].X;
        int ySpawnCoordinate = _playerPositions[playerUniqueName].Y;

        GodotDictionary initialWorldDictionary = new();
        initialWorldDictionary.Add("Width", _width);
        initialWorldDictionary.Add("Height", _height);


        RpcId(peerId, nameof(Initialize), initialWorldDictionary);
    }

    #endregion

    #region Block Changes

    // private void OnActiveBlockTakenDamage(int xPosition, int yPosition, float damageAmount) {
    //     RpcId(MultiplayerManager.HOST_ID, nameof(DamageSavedBlock),
    //         xPosition, yPosition, damageAmount);
    // }
    //
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    // private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
    //     SavedBlock savedBlock = _world.SavedBlocks[xPosition, yPosition];
    //     savedBlock?.TakeDamage(damageAmount);
    // }
    //
    // private void OnSavedBlockDestroyed(int xPosition, int yPosition) {
    //     BlockType blockType = _world.SavedBlocks[xPosition, yPosition].BlockType;
    //     _world.SavedBlocks[xPosition, yPosition] = null;
    //     EmitSignal(SignalName.ServerDeletedActiveBlock, blockType, xPosition, yPosition);
    // }
    //
    // private void OnPlayerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
    //     RpcId(MultiplayerManager.HOST_ID, nameof(ServerAttemptBuildBlock),
    //         xPosition, yPosition, blockResourcePath);
    // }
    //
    //
    // [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    // private void ServerAttemptBuildBlock(int xPosition, int yPosition, string blockResourcePath) {
    //     BlockType blockType = BlockType.Deserialize(blockResourcePath);
    //     if (!_world.AreCoordsInBounds(xPosition, yPosition)) return;
    //     if (_world.SavedBlocks[xPosition, yPosition] is not null) return;
    //     SavedBlock savedBlock = SavedBlock.Builder.New(blockType, xPosition, yPosition).Build();
    //     _world.SavedBlocks[xPosition, yPosition] = savedBlock;
    // }

    #endregion block changes
}