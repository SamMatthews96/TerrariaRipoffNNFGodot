using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    public const int HostId = 1;
    private World _world;

    public event Action Loaded;
    public event Action ExitGameStarted;
    public event Action ExitGameFinished;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;
    private Dictionary _playerData;

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _world = World.Create(_playerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(_world);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        _playerData = FileManager.LoadPlayer(playerData);
        _world = World.Create(_playerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(_world);

        Multiplayer.PeerConnected += OnPeerConnected;
        TreeExiting += () => { Multiplayer.PeerConnected -= OnPeerConnected; };
    }

    private void OnPeerConnected(long id) {
        Dictionary currentWorldData = _world.SerializeWorld();
        RpcId(id, nameof(PeerReceivedWorldData), currentWorldData);
    }

    public void InitAsClient(Dictionary playerData) {
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);

        Multiplayer.ServerDisconnected += OnServerDisconnected;
        TreeExiting += () => { Multiplayer.ServerDisconnected -= OnServerDisconnected; };

        _playerData = FileManager.LoadPlayer(playerData);
    }

    [Rpc]
    private void PeerReceivedWorldData(Dictionary worldData) {
        _world = World.Create(_playerData, worldData);
        _world.GameLoaded += OnWorldLoaded;
        AddChild(_world);
        _world.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
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