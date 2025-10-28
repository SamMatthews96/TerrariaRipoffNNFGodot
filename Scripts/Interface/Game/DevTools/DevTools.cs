using Godot;

namespace TerrariaRipoffNNF.Interface.DevTools;

public partial class DevTools : PanelContainer {
    [Export] private Game _gameInterface;

    public override void _Ready() {
        Visible = false;
        _gameInterface.GameManager.InputManager.ToggleDevToolsPressed +=
            OnToggleDevToolsPressed;
    }

    public override void _ExitTree() {
        _gameInterface.GameManager.InputManager.ToggleDevToolsPressed -=
            OnToggleDevToolsPressed;
    }

    private void OnToggleDevToolsPressed() {
        Visible = !Visible;
    }
}