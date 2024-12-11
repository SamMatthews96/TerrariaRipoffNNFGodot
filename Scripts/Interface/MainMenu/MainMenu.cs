using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
namespace TerrariaRipoffNNF;

public partial class MainMenu : Control {
    [Export] private Control _mainMenu;
    [Export] private Control _multiplayerMenu;
    [Export] private Control _worldMenu;
    [Export] private Control _joinMenu;
    [Export] private VBoxContainer _worldListContainer;
    [Export] private PackedScene _packedWorldListItem;
    [Export] private LineEdit _worldNameEdit;
    [Export] private LineEdit _ipEdit;
    private readonly List<Control> _menus = new();
    private GameType _gameType;
    [Export] private WorldCreator _worldCreator;

    private enum GameType {
        SinglePlayer,
        Host,
        Client
    }

    [Signal]
    public delegate void SinglePlayerClickedEnterWorldEventHandler(
        Dictionary world, Dictionary playerInfo);
    
    [Signal]
    public delegate void HostClickedEnterWorldEventHandler(
        Dictionary world, Dictionary playerInfo);
    
    [Signal]
    public delegate void ClientEnteredWorldEventHandler(
        string ipText, Dictionary playerInfo);

    public override void _Ready() {
        _menus.Add(_mainMenu);
        _menus.Add(_multiplayerMenu);
        _menus.Add(_worldMenu);
        _menus.Add(_joinMenu);
        ChangeToMenu(_mainMenu);

        Task<WorldBasicInfo[]> task = Task.Run(FileManager.LoadAllWorldBasicData);
        task.GetAwaiter().OnCompleted(() => {
            WorldBasicInfo[] worldBasicInfoArray = task.Result;
            foreach (WorldBasicInfo worldBasicInfo in worldBasicInfoArray) {
                AddEnterWorldButton(worldBasicInfo);
            }
        });
    }

    private void ChangeToMenu(Control menu) {
        foreach (Control menuToDisable in _menus) {
            menuToDisable.Hide();
        }

        menu.Show();
    }

    #region MenuMenu EventHandlers

    private void OnMainMenuSinglePlayerButtonDown() {
        _gameType = GameType.SinglePlayer;
        ChangeToMenu(_worldMenu);
    }

    private void OnMainMenuMultiplayerButtonDown() {
        ChangeToMenu(_multiplayerMenu);
    }

    private void OnMainMenuExitButtonDown() {
        GetTree().Quit();
    }

    #endregion

    #region WorldMenu EventHandlers

    private async void OnWorldMenuCreateWorldButtonDown() {
        WorldBasicInfo worldBasicInfo = new(_worldNameEdit.Text, 100, 100);
        await Task.Run(() => {
            _worldCreator.CreateWorld(worldBasicInfo);
        });
        AddEnterWorldButton(worldBasicInfo);
    }

    private void OnWorldMenuBackButtonDown() {
        switch (_gameType) {
            case GameType.SinglePlayer:
                ChangeToMenu(_mainMenu);
                break;
            case GameType.Host:
                ChangeToMenu(_multiplayerMenu);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void OnWorldMenuSelectWorldButtonDown(WorldBasicInfo worldBasicInfo) {
        Dictionary world = await Task.Run(() => FileManager.LoadWorld(worldBasicInfo));
        Dictionary playerInfo = new();
        playerInfo.Add("Name", "123-456");
        
        EmitSignal(
            _gameType == GameType.Host
                ? SignalName.HostClickedEnterWorld
                : SignalName.SinglePlayerClickedEnterWorld,
            world, playerInfo);
    }
    
    private async void OnWorldMenuDeleteWorldbuttonDown(WorldBasicInfo worldBasicInfo) {
        GD.Print("delete world");
        // await Task.Run(() => {
        //     FileManager.DeleteWorld(worldBasicInfo);
        // });
        // foreach (Control child in _worldListContainer.GetChildren()) {
        //     WorldListItem worldListItem = (WorldListItem) child;
        //     if (worldListItem.WorldBasicInfo == worldBasicInfo) {
        //         _worldListContainer.RemoveChild(worldListItem);
        //         break;
        //     }
        // }
    }

    #endregion

    #region MultiplayerMenu EventHandlers

    private void OnMultiplayerMenuHostButtonDown() {
        _gameType = GameType.Host;
        ChangeToMenu(_worldMenu);
    }

    private void OnMultiplayerMenuJoinButtonDown() {
        _gameType = GameType.Client;
        ChangeToMenu(_joinMenu);
    }

    private void OnMultiplayerMenuBackButtonDown() {
        ChangeToMenu(_mainMenu);
    }

    #endregion

    #region JoinMenu EventHandlers

    private void OnJoinMenuEnterWorldButtonDown() {
        Dictionary playerInfo = new();
        playerInfo.Add("Name", "654-321");
        
        // temporary ip address
        EmitSignal(SignalName.ClientEnteredWorld, "127.0.0.1", playerInfo);
    }

    private void OnJoinMenuBackButtonDown() {
        ChangeToMenu(_multiplayerMenu);
    }

    #endregion

    private void AddEnterWorldButton(WorldBasicInfo worldBasicInfo) {
        WorldListItem worldListItem = _packedWorldListItem.Instantiate<WorldListItem>();
        worldListItem.Initialize(worldBasicInfo);
        worldListItem.SelectWorldButtonDown += OnWorldMenuSelectWorldButtonDown;
        worldListItem.DeleteWorldButtonDown += OnWorldMenuDeleteWorldbuttonDown;
        _worldListContainer.AddChild(worldListItem);
    }
}