using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class GameMenu : Control {
    [Export] private Button _exitGameButton;

    public event Action ExitGameButtonDown;

    public override void _Ready() {
        Hide();
        _exitGameButton.ButtonDown += OnExitGameButtonDown;
        SceneManager.Instance.Game.InputManager.EscapePressed += OnEscapePressed;
    }

    public override void _ExitTree() {
        _exitGameButton.ButtonDown -= OnExitGameButtonDown;
        SceneManager.Instance.Game.InputManager.EscapePressed -= OnEscapePressed;
    }

    private void OnExitGameButtonDown() {
        ExitGameButtonDown?.Invoke();
    }

    private void OnEscapePressed() {
        if (Visible) {
            Hide();
        } else {
            Show();
        }
    }
}