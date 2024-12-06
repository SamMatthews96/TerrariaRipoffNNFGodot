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

    private ENetMultiplayerPeer _peer;


    private Game _game;
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
        _mainMenu.ClientEnteredWorld += OnMainMenuClientClickedEnterWorld;
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        LaunchGame(playerInfo, world);
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(_port);
        if (error != Error.Ok) {
            throw new Exception("error cannot host! [20240808.1336.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;

        LaunchGame(playerInfo, world);
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        Multiplayer.ConnectedToServer += () => { LaunchGame(playerInfo); };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }

    private void LaunchGame(Dictionary playerInfo, Dictionary world = null) {
        Game = _gameManagerPackedScene.Instantiate<Game>();
        AddChild(Game);
        Game.Initialize(playerInfo, world);

        _mainMenu.QueueFree();
    }
}