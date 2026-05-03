using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Interface;

namespace TerrariaRipoffNNF;

public partial class SceneManager : Node {
    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    
    [Export] private PackedScene _packedMainMenu;
    [Export] private PackedScene _packedGame;
    [Export] private PackedScene _packedLoadingScreen;
    
    private MainMenu _mainMenu;
    private Game _game;
    private LoadingScreen _loadingScreen;

    public override void _Ready() {
        CreateMainMenu();
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary worldData, Dictionary playerData) {
        CreateGame();
        _game.InitAsSinglePlayer(worldData, playerData);
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsHost(world, playerInfo);
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsClient(playerInfo);
    }

    private void CreateGame() {
        _mainMenu.QueueFree();
        _mainMenu = null;
        
        _game = _packedGame.Instantiate<Game>();
        AddChild(_game);
        _loadingScreen = _packedLoadingScreen.Instantiate<LoadingScreen>();
        AddChild(_loadingScreen);

        _game.Loaded += OnGameWorldLoaded;
        _game.ExitGameFinished += ExitGame;
    }

    private void OnGameWorldLoaded() {
        _game.Loaded -= OnGameWorldLoaded;
        _loadingScreen.QueueFree();
        _loadingScreen = null;
    }

    private void ExitGame() {
        CreateMainMenu();
        _game.ExitGameFinished -= ExitGame;
        _game.QueueFree();
    }

    private void CreateMainMenu() {
        _mainMenu = _packedMainMenu.Instantiate<MainMenu>();
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientClickedEnterWorld += OnMainMenuClientClickedEnterWorld;

        AddChild(_mainMenu);
    }
}