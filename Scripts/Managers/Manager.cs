using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

// @todo rename to SceneManager, to clarify what this class should be responsible for
public partial class Manager : Node {
    public const int HostId = 1;

    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    private MainMenu _mainMenu;

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
        CreateMainMenu();
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        // why does the button press pass this information
        // and should the ui have these properties
        
        //Create single player
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
        /*  @todo Create Game for all 3 modes,
            If its single player, 
            Game.CreateAsSingle(world)
                
                
            If host
            Game.CreateAsHost(world) 
            
         */
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
        Game = Game.Create();
        Game.Interface.GameMenu.ExitGameButtonDown += ExitGame;
        AddChild(Game);
    }

    private void ExitGame() {
        Game.QueueFree();
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