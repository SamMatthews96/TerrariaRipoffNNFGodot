using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    private World _world;

    public event Action Loaded;
    public event Action ExitGameStarted;
    public event Action ExitGameFinished;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;
    private GameUnloader _gameUnloader;

    private Dictionary _playerData;

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        _world = World.CreateAsHost(this, playerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(_world);
        _playerData = FileManager.LoadPlayer(playerData);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        _world = World.CreateAsHost(this, playerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        AddChild(_world);
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        _playerData = FileManager.LoadPlayer(playerData);

        MultiplayerApi.PeerConnectedEventHandler peerConnectedHandler = id => {
            Dictionary metadata = new();
            metadata["Width"] = worldData["Width"];
            metadata["Height"] = worldData["Height"];
            metadata["itemMap"] = _world.ItemIdBimap.ToDictionary();
            RpcId(id, nameof(RpcClientCreateWorld), metadata);
        };
        Multiplayer.PeerConnected += peerConnectedHandler;
        TreeExiting += () => { Multiplayer.PeerConnected -= peerConnectedHandler; };
    }

    public void InitAsClient(Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        TreeExiting += () => {
            Multiplayer.ServerDisconnected -= OnServerDisconnected;
        };
    }

    private void OnServerDisconnected() {
        ExitGameStarted?.Invoke();
        ExitGameFinished?.Invoke();
    }

    [Rpc]
    private void RpcClientCreateWorld(Dictionary metadata) {
        _world = World.CreateAsClient(this, _playerData, metadata);
        _world.GameLoaded += OnWorldLoaded;
        AddChild(_world);
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
    }

    private void OnExitGameButtonDown() {
        _world.Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;
        ExitGameStarted?.Invoke();

        if (_world.IsHost) {
            _gameUnloader = new GameUnloader(_world);
            _gameUnloader.SaveComplete += OnSaveComplete;
            AddChild(_gameUnloader);
        } else {
            ExitGameFinished?.Invoke();
        }
    }

    private void OnSaveComplete() {
        _gameUnloader.SaveComplete -= OnSaveComplete;
        _gameUnloader.QueueFree();
        _gameUnloader = null;
        ExitGameFinished?.Invoke();
    }

    private void OnWorldLoaded() {
        _world.GameLoaded -= OnWorldLoaded;
        Loaded?.Invoke();
    }
}