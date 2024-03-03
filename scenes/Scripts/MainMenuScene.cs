using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class MainMenuScene : Control {
    [Export] private Control mainMenu;
    [Export] private Control multiplayerMenu;
    [Export] private Control worldMenu;
    [Export] private Control joinMenu;
    private List<Control> menus = new();

    private bool isMultiplayerMode;
    private bool isHost;

    [Signal]
    public delegate void EnteredWorldSinglePlayerEventHandler();

    [Signal]
    public delegate void EnteredWorldHostEventHandler();

    [Signal]
    public delegate void EnteredWorldClientEventHandler();

    [Signal]
    public delegate void CreatedWorldEventHandler(World world);

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

    private void OnMultiPlayerButtonDown() {
        isMultiplayerMode = true;
        ChangeToMenu(multiplayerMenu);
    }

    private void OnExitButtonDown() {
        GetTree().Quit();
    }

    private void OnSinglePlayerButtonDown() {
        ChangeToMenu(worldMenu);
    }

    private void OnMultiPlayerMenuBackButtonDown() {
        isMultiplayerMode = false;
        ChangeToMenu(mainMenu);
    }

    private void OnHostButtonDown() {
        isHost = true;
        ChangeToMenu(worldMenu);
    }

    private void OnJoinButtonDown() {
        isHost = false;
        ChangeToMenu(joinMenu);
    }

    private void OnEnterWorldButtonDown() {
        if (isMultiplayerMode) {
            EmitSignal(SignalName.EnteredWorldHost);
        } else {
            EmitSignal(SignalName.EnteredWorldSinglePlayer);
        }
        Hide();
    }

    private void OnWorldMenuBackButtonDown() {
        ChangeToMenu(isMultiplayerMode ? multiplayerMenu : mainMenu);
    }

    private void OnJoinMenuBackButtonDown() {
        ChangeToMenu(multiplayerMenu);
    }

    private void OnJoinMenuEnterWorldButtonDown(string ip) {
        EmitSignal(SignalName.EnteredWorldClient, ip);
        Hide();
    }

    private void OnWorldCreated(World world) {
        EmitSignal(SignalName.CreatedWorld, world);
    }
}