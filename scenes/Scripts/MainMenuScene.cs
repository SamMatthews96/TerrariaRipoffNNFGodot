using Godot;
using System;
using System.Text;

public partial class MainMenuScene : Node {
    [Export] private Control MainMenu;
    [Export] private Control MultiplayerMenu;
    [Export] private Control WorldMenu;

    private bool isMultiplayerMode;

    private void OnSinglePlayerButtonDown() {
        MainMenu.Hide();
        WorldMenu.Show();
    }

    private void OnMultiPlayerButtonDown() {
        isMultiplayerMode = true;
        MainMenu.Hide();
        MultiplayerMenu.Show();
    }

    private void OnHostButtonDown() {
        MultiplayerMenu.Hide();
        WorldMenu.Show();
    }

    private void OnJoinButtonDown() {
        GD.Print("JOIN");
    }

    private void OnMultiPlayerMenuBackButtonDown() {
        isMultiplayerMode = false;
        MultiplayerMenu.Hide();
        MainMenu.Show();
    }

    private void OnEnterWorldButtonDown() {
        GD.Print("ENTER WORLD");
    }

    private void OnWorldMenuBackButtonDown() {
        WorldMenu.Hide();
        if (isMultiplayerMode) {
            MultiplayerMenu.Show();
        } else {
            MainMenu.Show();
        }
    }
}