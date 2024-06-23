using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using TerrariaRipoffNNF.UI.Scripts;
using TerrariaRipoffNNF.Utils;
using GodotDictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class WorldManager : Node {
    public static WorldManager Instance { get; private set; }

    private string _name;
    private PlayerInfo _playerInfo;
    private int _width;
    private int _height;
    private Dictionary<string, IntVector> _playerPositions = new();

    public IntVector DefaultSpawnPosition { get; private set; }

    [Export] private LocalObjectSpawnManager _localObjectSpawnManager;
    [Export] private PlayerManager _playerManager;
    [Export] private BlockManager _blockManager;
    [Export] private ItemPickupManager _itemPickupManager;
    
    [Signal]
    public delegate void InitializedEventHandler();

    public override void _EnterTree() {
        Instance = this;
    }
    
    public override void _Ready() {
        MultiplayerManager.Instance.ConnectedToServer += OnConnectedToServer;
        InputManager.Instance.SaveGamePressed += OnInputManagerSaveGamePressed;
        MainMenuScene.Instance.WorldLoaded += OnMainMenuSceneWorldLoaded;
    }

    #region Getters

    public GodotDictionary WorldToDictionary() {
        GodotDictionary worldDictionary = new();
        worldDictionary.Add("Name", _name);
        worldDictionary.Add("Width", _width);
        worldDictionary.Add("Height", _height);

        Array savedBlockArray = BlockManager.Instance.SavedBlocksToArray();
        worldDictionary.Add("SavedBlocks", savedBlockArray);
        worldDictionary.Add("PlayerPositions", new Array());
        worldDictionary.Add("DefaultSpawnPosition", DefaultSpawnPosition.ToSerialised());

        return worldDictionary;
    }


    public IntVector GetPlayerSpawnPosition() {
        return DefaultSpawnPosition;
    }

    public Vector2 GetWorldPositionFromCellCoordinates(int xCoordinate, int yCoordinate) {
        return new Vector2(xCoordinate * BlockManager.BLOCK_SIZE, yCoordinate * BlockManager.BLOCK_SIZE);
    }

    #endregion

    #region WorldCreation

    private void OnMainMenuSceneWorldLoaded(GodotDictionary worldDictionary, PlayerInfo playerInfo) {
        _playerManager.Initialize(playerInfo);
        Initialize(worldDictionary);

    }

    [Rpc(CallLocal = true)]
    private void Initialize(GodotDictionary worldDictionary) {
        _name = (string)worldDictionary["Name"];
        _width = (int)worldDictionary["Width"];
        _height = (int)worldDictionary["Height"];
        _playerPositions = new Dictionary<string, IntVector>();
        Array defaultSpawnPosition = worldDictionary["DefaultSpawnPosition"].AsGodotArray();
        DefaultSpawnPosition = new IntVector(
            (int)defaultSpawnPosition[0],
            (int)defaultSpawnPosition[1]);
        _localObjectSpawnManager.Initialize(_width, _height);
        _blockManager.Initialize(worldDictionary);
        _itemPickupManager.Initialize(worldDictionary);
        EmitSignal(SignalName.Initialized);
    }

    private void OnConnectedToServer(PlayerInfo playerInfo) {
        int peerId = Multiplayer.GetUniqueId();
        _playerManager.Initialize(playerInfo);
        RpcId(MultiplayerManager.HOST_ID, nameof(ServerInitialiseWorldForNewPlayer),
            peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServerInitialiseWorldForNewPlayer(int peerId) {
        GodotDictionary initialWorldDictionary = WorldToDictionary();
        RpcId(peerId, nameof(Initialize), initialWorldDictionary);
    }

    #endregion


    private async void OnInputManagerSaveGamePressed() {
        GodotDictionary worldDictionary = WorldToDictionary();
        await Task.Run(() =>
            FileManager.SaveWorld(worldDictionary));
    }
}