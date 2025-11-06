using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;

    public event Action ExitGameFinished;

    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }

    [Export] public Interface.Game Interface { get; private set; }

    [Export] public InputManager InputManager { get; private set; }

    //@todo set these dynamically on peer from worldInfo
    [Export] public int Width { get; private set; } = 100;
    [Export] public int Height { get; private set; } = 100;
    public int PeerId => Multiplayer.GetUniqueId();

    public IntVector DefaultSpawnPosition { get; private set; }

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    private bool _isPlayerSaved;
    private bool _isWorldSaved;

    [Export] public WorldObjectManager WorldObjectManager { get; private set; }


    public static Game Create() {
        return Data.PackedScenes.Game.Instantiate<Game>();
    }

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
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);

        Multiplayer.ConnectedToServer += () => {
            WorldObjectManager.SetGameAsClient(this, playerData);
            DefaultSpawnPosition = new IntVector(5, 5);
        };
    }

    private void CreateWorld(Dictionary worldData, Dictionary playerData) {
        Width = (int)worldData["Width"];
        Height = (int)worldData["Height"];
        WorldObjectManager.SetGameAsHost(this, worldData, playerData);

        DefaultSpawnPosition = new IntVector(
            worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsInt32(),
            worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsInt32());

        _playerData = FileManager.LoadPlayer(playerData);
    }

    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitClicked;
    }

    public override void _ExitTree() {
        Interface.GameMenu.ExitGameButtonDown -= OnExitClicked;
    }

    private void OnExitClicked() {
        
        // Save World (done)
        // remember that we don't need to save the world on the client
        // Save Player (done)
        // Start clearing up worldObjects (todo)
        Player.PlayerSaved += OnPlayerSaved;
        WorldObjectManager.WorldSaved += OnWorldSaved;
        // once all are done, QueueFree() and load main menu

    }

    private void OnWorldSaved() {
        _isWorldSaved = true;
        TryExitGame();
    }

    private void OnPlayerSaved() {
        _isPlayerSaved = true;
        TryExitGame();
    }

    private void TryExitGame() {
        if (_isPlayerSaved && _isWorldSaved) {
            ExitGameFinished?.Invoke();
        }
    }

    public bool IsInBounds(IntVector intVector) {
        return intVector.X >= 0
               && intVector.X < Width
               && intVector.Y >= 0
               && intVector.Y < Height;
    }
}