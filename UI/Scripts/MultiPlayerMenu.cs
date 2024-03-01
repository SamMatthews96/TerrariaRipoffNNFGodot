using Godot;

namespace TerrariaRipoffNNF.UI.Scripts;

public partial class MultiplayerMenu : Control {
    [Signal]
    public delegate void HostButtonDownEventHandler();

    [Signal]
    public delegate void JoinButtonDownEventHandler();

    [Signal]
    public delegate void BackButtonDownEventHandler();

    private void OnHostButtonDown() {
        EmitSignal(SignalName.HostButtonDown);
    }

    private void OnJoinButtonDown() {
        EmitSignal(SignalName.JoinButtonDown);
    }

    private void OnBackButtonDown() {
        EmitSignal(SignalName.BackButtonDown);
    }
}