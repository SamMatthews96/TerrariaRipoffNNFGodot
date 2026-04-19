using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;

    public event Action ExitGameFinished;

    [Export] public Interface.Game Interface { get; private set; }

    [Export] public InputManager InputManager { get; private set; }

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    private bool _isPlayerSaved;
    private bool _isWorldSaved;

    [Export] public World World { get; private set; }

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        CreateWorld(worldData, playerData);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        CreateWorld(worldData, playerData);
    }

    public void InitAsClient(Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _multiplayerClient = new MultiplayerClient(this);
        AddChild(_multiplayerClient);

        Multiplayer.ConnectedToServer += () => {
            World.SetGameAsClient(this, playerData);
        };
        
    }

    private void CreateWorld(Dictionary worldData, Dictionary playerData) {
        World.SetGameAsHost(this, worldData, playerData);
        _playerData = FileManager.LoadPlayer(playerData);
    }

    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
    }
    
    private void OnExitGameButtonDown() {
        Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;
        ExitGameFinished?.Invoke();
    }
}