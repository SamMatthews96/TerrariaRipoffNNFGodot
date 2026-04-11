using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;

    public event Action ExitGameFinished;

    [Export] public Interface.Game Interface { get; private set; }

    [Export] public InputManager InputManager { get; private set; }

    public int PeerId => Multiplayer.GetUniqueId();

    public Vector2I DefaultSpawnPosition { get; private set; }

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
            DefaultSpawnPosition = new Vector2I(5, 5);
        };
    }

    private void CreateWorld(Dictionary worldData, Dictionary playerData) {
        World.SetGameAsHost(this, worldData, playerData);

        DefaultSpawnPosition = new Vector2I(
            worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsInt32(),
            worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsInt32()
        );

        _playerData = FileManager.LoadPlayer(playerData);
    }

    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitClicked;
    }

    public override void _ExitTree() {
        Interface.GameMenu.ExitGameButtonDown -= OnExitClicked;
    }

    private void OnExitClicked() {
        Player.PlayerSaved += OnPlayerSaved;
        World.WorldSaved += OnWorldSaved;
    }

    private void OnWorldSaved() {
        _isWorldSaved = true;
        World.WorldSaved -= OnWorldSaved;
        TryExitGame();
    }

    private void OnPlayerSaved() {
        _isPlayerSaved = true;
        Player.PlayerSaved -= OnPlayerSaved;
        TryExitGame();
    }

    private void TryExitGame() {
        if (!_isPlayerSaved) return;
        if (_multiplayerClient is null && !_isWorldSaved) return;
        GetTree().Paused = false;
        ExitGameFinished?.Invoke();
    }


}