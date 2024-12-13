using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class SelectGameTypeMenu : Control {
    [Export] private Button _singlePlayerButton;
    [Export] private Button _multiplayerButton;
    [Export] private Button _exitButton;

    public event Action SinglePlayerButtonDown;
    public event Action MultiplayerButtonDown;
    
    public override void _Ready() {
        Show();
        _singlePlayerButton.ButtonDown += OnSinglePlayerButtonDown;
        _multiplayerButton.ButtonDown += OnMultiplayerButtonDown;
        _exitButton.ButtonDown += OnExitButtonDown;
    }

    public override void _ExitTree() {
        _singlePlayerButton.ButtonDown -= OnSinglePlayerButtonDown;
        _multiplayerButton.ButtonDown -= OnMultiplayerButtonDown;
        _exitButton.ButtonDown -= OnExitButtonDown;
    }

    private void OnSinglePlayerButtonDown() {
        Hide();
        SinglePlayerButtonDown?.Invoke();
    }

    private void OnMultiplayerButtonDown() {
        Hide();
        MultiplayerButtonDown?.Invoke();
    }

    private void OnExitButtonDown() {
        GetTree().Quit();
    }
}