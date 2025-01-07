using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SceneManager : Node {
    public const int HostId = 1;

    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    private MainMenu _mainMenu;

    private ENetMultiplayerPeer _peer;
    private Game _game;

    public static SceneManager Instance { get; private set; }

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20241130.1855.1] Manager already instantiated");
        }

        Instance = this;
    }

    public override void _Ready() {
        CreateMainMenu();
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary worldData, Dictionary playerData) {
        _mainMenu.QueueFree();
        Game game = Game.CreateSinglePlayer(worldData, playerData);
        AddChild(game);
        
        _game.Interface.GameMenu.ExitGameButtonDown += ExitGame;
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(_port);
        if (error != Error.Ok) {
            throw new Exception("error cannot host! [20240808.1336.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;

        // CreateNewGame();
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        Multiplayer.ConnectedToServer += () => {
            // CreateNewGame();
        };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }

    private void ExitGame() {
        CreateMainMenu();
    }

    private void CreateMainMenu() {
        _mainMenu = Data.PackedScenes.MainMenu.Instantiate<MainMenu>();
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientClickedEnterWorld += OnMainMenuClientClickedEnterWorld;

        AddChild(_mainMenu);
    }
}