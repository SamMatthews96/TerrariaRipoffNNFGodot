using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class DevTools : PanelContainer {
    [Export] private Game _gameInterface;
    [Export] private Button _spawnButton;

    public event Action SpawnPressed;

    public override void _Ready() {
        Visible = false;
        _gameInterface.GameManager.InputManager.ToggleDevToolsPressed +=
            OnToggleDevToolsPressed;

        _spawnButton.Pressed += OnSpawnButtonPressed;
    }

    public override void _ExitTree() {
        _gameInterface.GameManager.InputManager.ToggleDevToolsPressed -=
            OnToggleDevToolsPressed;

        _spawnButton.Pressed -= OnSpawnButtonPressed;
    }

    private void OnSpawnButtonPressed() {
        SpawnPressed?.Invoke();
    }

    private void OnToggleDevToolsPressed() {
        Visible = !Visible;
    }
}