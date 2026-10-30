using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    public const int HostId = 1;
    private World _world;
    // generally, an rpc should go to all peers.
    // but, if a peer hasn't been fully connected, we need to buffer
    // the rpc until the peer is connected.

    public event Action Loaded;
    public event Action ExitGameStarted;
    public event Action ExitGameFinished;
    public event Action<Dictionary> PeerConnected;
    public event Action<int> AfterPeerConnected;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;
    private Dictionary _localPlayerData;

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        RpcHandler.Init(this);
        _localPlayerData = FileManager.LoadPlayer(playerData);
        _world = World.Create(this, _localPlayerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(_world);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);
        
        RpcHandler.Init(this);
        _localPlayerData = FileManager.LoadPlayer(playerData);
        _world = World.Create(this, _localPlayerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(_world);
    }

    public void InitAsClient(Dictionary playerData) {
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);

        RpcHandler.Init(this);
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
        TreeExiting += () => {
            Multiplayer.ConnectedToServer -= OnConnectedToServer;
            Multiplayer.ServerDisconnected -= OnServerDisconnected;
        };

        _localPlayerData = FileManager.LoadPlayer(playerData);
    }

    private void OnConnectedToServer() {
        this.RpcHost(nameof(ProcessNewPeer), _localPlayerData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ProcessNewPeer(Dictionary playerData) {
        int senderId = Multiplayer.GetRemoteSenderId();
        
        RpcHandler.AddNewPeer(senderId);
        PeerConnected?.Invoke(playerData);
        Dictionary currentWorldData = _world.SerializeWorld();
        RpcId(senderId, nameof(PeerReceivedWorldData), currentWorldData);
    }

    [Rpc]
    private void PeerReceivedWorldData(Dictionary worldData) {
        // simulate a delay
        Timer timer = new();
        AddChild(timer);
        timer.WaitTime = 5f;
        timer.Start();
        timer.Timeout += () => {
            timer.QueueFree();
            _world = World.Create(this, _localPlayerData, worldData);
            _world.GameLoaded += OnWorldLoaded;
            AddChild(_world);
            _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
            this.RpcHost(nameof(AfterPeerReceivedWorldData));
        };
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void AfterPeerReceivedWorldData() {
        int senderId = Multiplayer.GetRemoteSenderId();
        RpcHandler.RemoveNewPeer(senderId);
        AfterPeerConnected?.Invoke(senderId);
    }

    private void OnServerDisconnected() {
        ExitGameStarted?.Invoke();
        ExitGameFinished?.Invoke();
    }

    private void OnExitGameButtonDown() {
        _world.Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;
        ExitGameStarted?.Invoke();

        if (_world.IsHost) {
            _world.ProcessMode = ProcessModeEnum.Disabled;
            SaveGame();
        } else {
            ExitGameFinished?.Invoke();
        }
    }

    private async void SaveGame() {
        try {
            Dictionary worldData = _world.SerializeWorld();
            await Task.Run(() => FileManager.SaveWorld(worldData));
        } catch (Exception e) {
            GD.PrintErr("Failed to save world: " + e + "");
        } finally {
            ExitGameFinished?.Invoke();
        }
    }

    private void OnWorldLoaded() {
        _world.GameLoaded -= OnWorldLoaded;
        Loaded?.Invoke();
    }
}