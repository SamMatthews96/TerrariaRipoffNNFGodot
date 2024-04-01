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
    private Dictionary<string, IntVector> _playerPositions = new();

    public IntVector DefaultSpawnPosition { get; private set; }

    [Export] private LocalObjectSpawnManager localObjectSpawnManager;
    [Export] private PlayerManager playerManager;

    [Signal]
    public delegate void InitializedEventHandler();
    
    [Signal] public delegate void SavedBlockDestroyedEventHandler(int xPosition, int yPosition);
    public override void _Ready() {
        Instance = this;
    }

    #region Getters

    public List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = new();
        foreach (IntVector coords in region) {
            SavedBlock savedBlock = _savedBlocks[coords.X, coords.Y];
            if (savedBlock is null) continue;
            savedBlocks.Add(savedBlock);
        }
        return savedBlocks;
    }
    
    public IntVector GetPlayerSpawnPosition() {
        return DefaultSpawnPosition;
    }

    public Vector2 GetWorldPositionFromCellCoordinates(int xCoordinate, int yCoordinate) {
        return new Vector2(xCoordinate * BLOCK_SIZE, yCoordinate * BLOCK_SIZE);
    }

    #endregion

    #region WorldCreation

    private void OnMainMenuSceneWorldLoaded(GodotDictionary worldDictionary, PlayerInfo playerInfo) {
        playerManager.Initialize(playerInfo);
        Initialize(worldDictionary);

        foreach (SavedBlock savedBlock in _savedBlocks) {
            if (savedBlock is null) continue;
            savedBlock.HitZeroHealth += OnSavedBlockHitZeroHealth;
        }
    }

    [Rpc(CallLocal = true)]
    private void Initialize(GodotDictionary worldDict) {
        _width = worldDict["Width"].ToString().ToInt();
        _height = worldDict["Height"].ToString().ToInt();
        Array savedBlockArray = worldDict["SavedBlocks"].AsGodotArray();
        _savedBlocks = new SavedBlock[_width, _height];
        foreach (GodotDictionary savedBlockDictionary in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }

        _playerPositions = new Dictionary<string, IntVector>();
        Array defaultSpawnPosition = worldDict["DefaultSpawnPosition"].AsGodotArray();
        DefaultSpawnPosition = new IntVector(
            defaultSpawnPosition[0].ToString().ToInt(),
            defaultSpawnPosition[1].ToString().ToInt());
        localObjectSpawnManager.Initialize(_width, _height);
        EmitSignal(SignalName.Initialized);
    }

    private void OnConnectedToServer(PlayerInfo playerInfo) {
        int peerId = Multiplayer.GetUniqueId();
        playerManager.Initialize(playerInfo);
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerInitialiseWorldForNewPlayer),
            peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServerInitialiseWorldForNewPlayer(int peerId) {
        GodotDictionary initialWorldDictionary = new();
        initialWorldDictionary.Add("Width", _width);
        initialWorldDictionary.Add("Height", _height);

        Array savedBlockArray = new();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                SavedBlock savedBlock = _savedBlocks[x, y];
                if (savedBlock is null) continue;
                savedBlockArray.Add(savedBlock.Serialize());
            }
        }

        initialWorldDictionary.Add("SavedBlocks", savedBlockArray);
        initialWorldDictionary.Add("PlayerPositions", new Array());
        initialWorldDictionary.Add("DefaultSpawnPosition",
            new Array { DefaultSpawnPosition.X, DefaultSpawnPosition.Y });

        RpcId(peerId, nameof(Initialize), initialWorldDictionary);
    }

    #endregion

    #region Block Changes

    public void PeerActiveBlockTakenDamage(int xPosition, int yPosition, float damageAmount) {
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerDamageSavedBlock),
            xPosition, yPosition, damageAmount);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerDamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        Rpc(nameof(DamageSavedBlock), xPosition, yPosition, damageAmount);
    }
    
    [Rpc(CallLocal = true)]
    private void DamageSavedBlock(int xPosition, int yPosition, float damageAmount) {
        SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
        savedBlock?.TakeDamage(damageAmount);
    }
    
    private void OnSavedBlockHitZeroHealth(int xPosition, int yPosition) {
        Rpc(nameof(DestroySavedBlock), xPosition, yPosition);
    }
    
    [Rpc(CallLocal = true)]
    private void DestroySavedBlock(int xPosition, int yPosition) {
        SavedBlock savedBlock = _savedBlocks[xPosition, yPosition];
        if (savedBlock is null) return;
        _savedBlocks[xPosition, yPosition] = null;
        EmitSignal(SignalName.SavedBlockDestroyed, xPosition, yPosition);
    }
    
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