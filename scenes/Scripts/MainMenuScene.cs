using System;
using Godot;
using Microsoft.VisualBasic;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class MainMenuScene : Node {
    [Export] private Control mainMenu;
    [Export] private Control multiplayerMenu;
    [Export] private Control worldMenu;

    [Export] private PackedScene packedGameManager;

    private bool isMultiplayerMode;
    private bool isHost;

    [Signal]
    public delegate void EnteredWorldSinglePlayerEventHandler();

    [Signal]
    public delegate void EnteredWorldHostEventHandler();

    [Signal]
    public delegate void EnteredWorldClientEventHandler();

    private void OnSinglePlayerButtonDown() {
        mainMenu.Hide();
        worldMenu.Show();
    }

    private void OnMultiPlayerButtonDown() {
        isMultiplayerMode = true;
        mainMenu.Hide();
        multiplayerMenu.Show();
    }

    private void OnHostButtonDown() {
        isHost = true;
        multiplayerMenu.Hide();
        worldMenu.Show();
    }

    private void OnJoinButtonDown() {
        isHost = false;
        multiplayerMenu.Hide();
        worldMenu.Show();
    }

    private void OnMultiPlayerMenuBackButtonDown() {
        isMultiplayerMode = false;
        multiplayerMenu.Hide();
        mainMenu.Show();
    }

    private void OnEnterWorldButtonDown() {
        if (!isMultiplayerMode) {
            EmitSignal(SignalName.EnteredWorldSinglePlayer);
        } else if (isHost) {
            EmitSignal(SignalName.EnteredWorldHost);
        } else {
            EmitSignal(SignalName.EnteredWorldClient);
        }
    }

    private void OnWorldMenuBackButtonDown() {
        worldMenu.Hide();
        if (isMultiplayerMode) {
            multiplayerMenu.Show();
        } else {
            mainMenu.Show();
        }
    }

    private void OnExitButtonDown() {
        GetTree().Quit();
    }
}