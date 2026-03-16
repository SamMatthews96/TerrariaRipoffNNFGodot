using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SceneManager : Node {
    public const int HostId = 1;

    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    private MainMenu _mainMenu;

    private Game _game;
    private Node _loadingScreen;

    public override void _Ready() {
        CreateMainMenu();
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary worldData, Dictionary playerData) {
        CreateGame();
        _game.InitAsSinglePlayer(worldData, playerData);
        _game.World.WorldLoadedLocally += OnWorldLoadedLocally;
    }

    private void CreateGame() {
        _mainMenu.QueueFree();
        _loadingScreen = Data.PackedScenes.LoadingScreen.Instantiate();
        AddChild(_loadingScreen);
        _game = Game.Create();
        AddChild(_game);

        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
        _game.ExitGameFinished += ExitGame;
    }

    private void OnExitGameClicked() {
        _loadingScreen = Data.PackedScenes.LoadingScreen.Instantiate();
        AddChild(_loadingScreen);
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
    }

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsHost(world, playerInfo);
        _game.World.WorldLoadedLocally += OnWorldLoadedLocally;
    }

    private void OnWorldLoadedLocally() {
        _loadingScreen.QueueFree();
        _loadingScreen = null;
    }

    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsClient(playerInfo);
        _game.World.WorldLoadedLocally += OnWorldLoadedLocally;
    }

    private void ExitGame() {
        CreateMainMenu();
        _game.ExitGameFinished -= ExitGame;
        _game.QueueFree();

        _loadingScreen.QueueFree();
        _loadingScreen = null;
    }

    private void CreateMainMenu() {
        _mainMenu = Data.PackedScenes.MainMenu.Instantiate<MainMenu>();
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientClickedEnterWorld += OnMainMenuClientClickedEnterWorld;

        AddChild(_mainMenu);
    }
}