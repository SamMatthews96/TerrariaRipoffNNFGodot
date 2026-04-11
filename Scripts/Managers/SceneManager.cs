using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SceneManager : Node {
    public const int HostId = 1;

    [Export] private int _port = 8910;
    [Export] private string _address = "127.0.0.1";
    private MainMenu _mainMenu;

    private Game _game;

    public override void _Ready() {
        CreateMainMenu();
    }

    private void OnMainMenuSinglePlayerClickedEnterWorld(Dictionary worldData, Dictionary playerData) {
        CreateGame();
        _game.InitAsSinglePlayer(worldData, playerData);
    }

    private void CreateGame() {
        _mainMenu.QueueFree();
        _game = Data.PackedScenes.Game.Instantiate<Game>();
        AddChild(_game);

        _game.ExitGameFinished += ExitGame;
    }

   

    private void OnMainMenuHostClickedEnterWorld(Dictionary world, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsHost(world, playerInfo);
    }


    private void OnMainMenuClientClickedEnterWorld(string ip, Dictionary playerInfo) {
        CreateGame();
        _game.InitAsClient(playerInfo);
    }

    private void ExitGame() {
        CreateMainMenu();
        _game.ExitGameFinished -= ExitGame;
        _game.QueueFree();
    }

    private void CreateMainMenu() {
        _mainMenu = Data.PackedScenes.MainMenu.Instantiate<MainMenu>();
        _mainMenu.SinglePlayerClickedEnterWorld += OnMainMenuSinglePlayerClickedEnterWorld;
        _mainMenu.HostClickedEnterWorld += OnMainMenuHostClickedEnterWorld;
        _mainMenu.ClientClickedEnterWorld += OnMainMenuClientClickedEnterWorld;

        AddChild(_mainMenu);
    }
}