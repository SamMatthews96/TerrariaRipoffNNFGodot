using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    public World World { get; private set; }

    public event Action Loaded;
    public event Action ExitGameFinished;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        World = World.CreateAsHost(this, worldData, playerData);
        World.GameLoaded += OnWorldLoaded;
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        AddChild(World);
        _playerData = FileManager.LoadPlayer(playerData);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        World = World.CreateAsHost(this, worldData, playerData);
        World.GameLoaded += OnWorldLoaded;
        AddChild(World);
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
        _playerData = FileManager.LoadPlayer(playerData);

       MultiplayerApi.PeerConnectedEventHandler peerConnectedHandler = id => {
            Dictionary metadata = new();
            metadata["Width"] = worldData["Width"];
            metadata["Height"] = worldData["Height"];
            metadata["itemMap"] = World.ItemIdBimap.ToDictionary();
            RpcId(id, nameof(RpcClientCreateWorld), metadata);
        };
        Multiplayer.PeerConnected += peerConnectedHandler;
        TreeExiting += () => {
            Multiplayer.PeerConnected -= peerConnectedHandler;
        };
    }

    public void InitAsClient(Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);
    }

    [Rpc]
    private void RpcClientCreateWorld(Dictionary metadata) {
        World = World.CreateAsClient(metadata, _playerData, this);
        World.GameLoaded += OnWorldLoaded;
        AddChild(World);
        World.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
    }
    
    private void OnExitGameButtonDown() {
        World.Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;
        ExitGameFinished?.Invoke();
    }
    
    private void OnWorldLoaded() {
        World.GameLoaded -= OnWorldLoaded;
        Loaded?.Invoke();
    }
}