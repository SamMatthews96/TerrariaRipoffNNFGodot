using System;
using Godot;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class MainMenuScene : Node {
    [Export] private Control mainMenu;
    [Export] private Control multiplayerMenu;
    [Export] private Control worldMenu;

    [Export] private PackedScene packedGameManagerSinglePlayer;
    [Export] private PackedScene packedGameManagerHost;
    [Export] private PackedScene packedGameManagerClient;

    private bool isMultiplayerMode;
    private bool isHost;

    [Signal]
    public delegate void StartedSinglePlayerGameEventHandler();

    [Signal]
    public delegate void StartedHostingMultiplayerGameEventHandler();

    [Signal]
    public delegate void JoiningMultiplayerGameEventHandler();

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
        Node gameManager;
        if (!isMultiplayerMode) {
            gameManager = packedGameManagerSinglePlayer.Instantiate();
        } else if (isHost) {
            gameManager = packedGameManagerHost.Instantiate();
        } else {
            gameManager = packedGameManagerClient.Instantiate();
        }

        GetTree().Root.AddChild(gameManager);
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