using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class MainMenuScene : Control {
    [Export] private Control mainMenu;
    [Export] private Control multiplayerMenu;
    [Export] private Control worldMenu;
    [Export] private Control joinMenu;
    [Export] private VBoxContainer worldListContainer;
    [Export] private PackedScene packedEnterWorldButton;
    [Export] private LineEdit worldNameEdit;
    [Export] private LineEdit ipEdit;
    private readonly List<Control> menus = new();
    private GameType gameType;

    private enum GameType {
        SinglePlayer,
        Host,
        Client
    }

    [Signal]
    public delegate void WorldLoadedEventHandler(World world);

    [Signal]
    public delegate void WorldLoadedHostModeEventHandler();

    [Signal]
    public delegate void JoinGameButtonDownEventHandler(string ipText);

    public override void _Ready() {
        menus.Add(mainMenu);
        menus.Add(multiplayerMenu);
        menus.Add(worldMenu);
        menus.Add(joinMenu);
        ChangeToMenu(mainMenu);

        Task<WorldBasicInfo[]> task = Task.Run(FileManager.LoadAllWorldBasicData);
        task.GetAwaiter().OnCompleted(() => {
            WorldBasicInfo[] worldBasicInfoArray = task.Result;
            foreach (WorldBasicInfo worldBasicInfo in worldBasicInfoArray) {
                AddEnterWorldButton(worldBasicInfo);
            }
        });
    }

    private void ChangeToMenu(Control menu) {
        foreach (Control menuToDisable in menus) {
            menuToDisable.Hide();
        }

        menu.Show();
    }

    #region MenuMenu EventHandlers

    private void OnMainMenuSinglePlayerButtonDown() {
        gameType = MainMenuScene.GameType.SinglePlayer;
        ChangeToMenu(worldMenu);
    }

    private void OnMainMenuMultiplayerButtonDown() {
        ChangeToMenu(multiplayerMenu);
    }

    private void OnMainMenuExitButtonDown() {
        GetTree().Quit();
    }

    #endregion

    #region WorldMenu EventHandlers

    private async void OnWorldMenuCreateWorldButtonDown() {
        WorldBasicInfo worldBasicInfo = new(worldNameEdit.Text, 100, 100);
        World world = await Task.Run(() => WorldCreator.CreateWorld(worldBasicInfo));
        AddEnterWorldButton(worldBasicInfo);
        await Task.Run(() => FileManager.SaveWorld(world));
    }

    private void OnWorldMenuBackButtonDown() {
        switch (gameType) {
            case GameType.SinglePlayer:
                ChangeToMenu(mainMenu);
                break;
            case GameType.Host:
                ChangeToMenu(multiplayerMenu);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async void OnWorldMenuEnterWorldButtonDown(WorldBasicInfo worldBasicInfo) {
        World world = await Task.Run(() => FileManager.LoadWorld(worldBasicInfo));
        if (gameType == GameType.Host) {
            EmitSignal(SignalName.WorldLoadedHostMode);
        }

        EmitSignal(SignalName.WorldLoaded, world);
        QueueFree();
    }

    #endregion

    #region MultiplayerMenu EventHandlers

    private void OnMultiplayerMenuHostButtonDown() {
        gameType = MainMenuScene.GameType.Host;
        ChangeToMenu(worldMenu);
    }

    private void OnMultiplayerMenuJoinButtonDown() {
        gameType = MainMenuScene.GameType.Client;
        ChangeToMenu(joinMenu);
    }

    private void OnMultiplayerMenuBackButtonDown() {
        ChangeToMenu(mainMenu);
    }

    #endregion

    #region JoinMenu EventHandlers

    private void OnJoinMenuEnterWorldButtonDown() {
        EmitSignal(SignalName.JoinGameButtonDown, ipEdit.Text);
    }

    private void OnJoinMenuBackButtonDown() {
        ChangeToMenu(multiplayerMenu);
    }

    #endregion

    private void AddEnterWorldButton(WorldBasicInfo worldBasicInfo) {
        EnterWorldButton enterWorldButton = packedEnterWorldButton.Instantiate<EnterWorldButton>();
        enterWorldButton.Initialize(worldBasicInfo);
        enterWorldButton.EnterWorldButtonDown += OnWorldMenuEnterWorldButtonDown;
        worldListContainer.AddChild(enterWorldButton);
    }

    private void OnStartedGame() {
        QueueFree();
    }

    private void OnConnectedToServer() {
        QueueFree();
    }
}