using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class GameMenu : Control {
    [Export] private Game _gameInterface;
    [Export] private Button _exitGameButton;

    public event Action ExitGameButtonDown;

    public override void _Ready() {
        Hide();
        _exitGameButton.ButtonDown += OnExitGameButtonDown;
        _gameInterface.GameManager.InputManager.EscapePressed += OnEscapePressed;
    }

    public override void _ExitTree() {
        _exitGameButton.ButtonDown -= OnExitGameButtonDown;
        _gameInterface.GameManager.InputManager.EscapePressed -= OnEscapePressed;
    }

    private void OnExitGameButtonDown() {
        ExitGameButtonDown?.Invoke();
    }

    private void OnEscapePressed() {
        if (
            _gameInterface.CraftingInterface.Visible ||
            _gameInterface.InventoryUi.Visible ||
            _gameInterface.PlayerEquipment.Visible
        ) {
            return;
        }

        if (Visible) {
            Hide();
        } else {
            Show();
        }
    }
}