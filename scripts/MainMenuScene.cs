using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;

namespace TerrariaRipoffNNF.Scripts;

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

    public enum GameType {
        SinglePlayer,
        Host,
        Client
    }

    [Signal]
    public delegate void CreateWorldButtonDownEventHandler(string worldName);

    [Signal]
    public delegate void EnterWorldAsHostButtonDownEventHandler(WorldBasicInfo worldBasicInfo);

    [Signal]
    public delegate void EnterWorldAsSingleButtonDownEventHandler(WorldBasicInfo worldBasicInfo);

    [Signal]
    public delegate void EnterWorldAsClientButtonDownEventHandler();

    private void ChangeToMenu(Control menu) {
        foreach (Control menuToDisable in menus) {
            menuToDisable.Hide();
        }

        menu.Show();
    }

    public override void _Ready() {
        menus.Add(mainMenu);
        menus.Add(multiplayerMenu);
        menus.Add(worldMenu);
        menus.Add(joinMenu);
        ChangeToMenu(mainMenu);
    }

    #region MenuMenu EventHandlers

    private void OnMainMenuSinglePlayerButtonDown() {
        gameType = GameType.SinglePlayer;
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

    private void OnWorldMenuCreateWorldButtonDown() {
        EmitSignal(SignalName.CreateWorldButtonDown, worldNameEdit.Text);
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

    private void OnWorldMenuEnterWorldButtonDown(WorldBasicInfo worldBasicInfo) {
        switch (gameType) {
            case GameType.SinglePlayer:
                EmitSignal(SignalName.EnterWorldAsSingleButtonDown, worldBasicInfo);
                break;
            case GameType.Host:
                EmitSignal(SignalName.EnterWorldAsHostButtonDown, worldBasicInfo);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #region MultiplayerMenu EventHandlers

    private void OnMultiplayerMenuHostButtonDown() {
        gameType = GameType.Host;
        ChangeToMenu(worldMenu);
    }

    private void OnMultiplayerMenuJoinButtonDown() {
        gameType = GameType.Client;
        ChangeToMenu(joinMenu);
    }

    private void OnMultiplayerMenuBackButtonDown() {
        ChangeToMenu(mainMenu);
    }

    #endregion

    #region JoinMenu EventHandlers

    private void OnJoinMenuEnterWorldButtonDown() {
        EmitSignal(SignalName.EnterWorldAsClientButtonDown, ipEdit.Text);
    }

    private void OnJoinMenuBackButtonDown() {
        ChangeToMenu(multiplayerMenu);
    }

    #endregion

    private void OnWorldBasicDataLoaded(Array worldBasicDataArray) {
        foreach (Dictionary worldBasicDataDict in worldBasicDataArray) {
            WorldBasicInfo worldBasicInfo = WorldBasicInfo.FromDict(worldBasicDataDict);
            AddEnterWorldButton(worldBasicInfo);
        }
    }

    private void OnWorldCreatorWorldCreated(World world) {
        WorldBasicInfo worldBasicInfo = world.GetBasicInfo();
        AddEnterWorldButton(worldBasicInfo);
    }

    private void AddEnterWorldButton(WorldBasicInfo worldBasicInfo) {
        EnterWorldButton enterWorldButton = packedEnterWorldButton.Instantiate<EnterWorldButton>();
        enterWorldButton.Initialize(worldBasicInfo);
        enterWorldButton.EnterWorldButtonDown += OnWorldMenuEnterWorldButtonDown;
        worldListContainer.AddChild(enterWorldButton);
    }

    private void OnStartedGame() {
        QueueFree();
    }

    private void OnGameManagerConnectedToServer() {
        QueueFree();
    }
}