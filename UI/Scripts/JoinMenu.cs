using Godot;

namespace TerrariaRipoffNNF.UI.Scripts;

public partial class JoinMenu : Control {
    [Export] private LineEdit ipInput;
    
    [Signal]
    public delegate void EnterWorldButtonDownEventHandler(string ipInputValue);

    [Signal]
    public delegate void BackButtonDownEventHandler();

    private void OnEnterWorldButtonDown() {
        string temp = ipInput.Text == "" ? "127.0.0.1" : ipInput.Text;
        EmitSignal(SignalName.EnterWorldButtonDown, temp);
    }

    private void OnBackButtonDown() {
        EmitSignal(SignalName.BackButtonDown);
    }
}