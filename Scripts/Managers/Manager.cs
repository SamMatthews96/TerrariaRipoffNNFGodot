using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Manager : Node {
    public const int HostId = 1;

    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    [Export] private PackedScene _gameManagerPackedScene;
    [Export] private MainMenu _mainMenu;

    [Export] public PackedScenes PackedScenes { get; private set; }
    private ENetMultiplayerPeer _peer;
    private Game _game;

    public event Action<Dictionary> LaunchedGameAsHost;
    public event Action<Dictionary> JoinedGame;

    public Game Game {
        get => _game ?? throw new Exception("[20241205.2000.8] Game not instantiated");
        private set => _game = value;
    }

    public static Manager Instance { get; private set; }

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20241130.1855.1] Manager already instantiated");
        }

        Instance = this;
    }

    public override void _Ready() {
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientClickedEnterWorld += OnMainMenuClientClickedEnterWorld;
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        CreateNewGame();
        LaunchedGameAsHost?.Invoke(world);
        JoinedGame?.Invoke(playerInfo);
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(_port);
        if (error != Error.Ok) {
            throw new Exception("error cannot host! [20240808.1336.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;

        CreateNewGame();
        LaunchedGameAsHost?.Invoke(world);
        JoinedGame?.Invoke(playerInfo);
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        Multiplayer.ConnectedToServer += () => {
            CreateNewGame();
            JoinedGame?.Invoke(playerInfo);
        };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }

    private void CreateNewGame() {
        _mainMenu.QueueFree();
        Game = _gameManagerPackedScene.Instantiate<Game>();
        Game.Interface.GameMenu.ExitGameButtonDown += ExitGame;
        AddChild(Game);
    }

    private void ExitGame() {
        Game.QueueFree();
        _mainMenu = PackedScenes.PackedMainMenu.Instantiate<MainMenu>();
        AddChild(_mainMenu);
    }

    private void CreateMainMenu() {
        
    }
}