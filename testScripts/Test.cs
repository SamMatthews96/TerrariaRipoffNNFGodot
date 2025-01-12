using Godot;

namespace TerrariaRipoffNNF;

public partial class Test : Node {
    [Export] private Button _popupButton;
    [Export] private PopupPanel _popupPanel;
    public override void _Ready() {
        _popupButton.ButtonDown += () => {
            if (_popupPanel.Visible) {
                _popupPanel.Hide();
            } else {
                _popupPanel.Show();
            }
        };
        
        _popupPanel.MouseEntered += () => {
            GD.Print("Mouse entered");
        };
        _popupPanel.MouseExited += () => {
            GD.Print("Mouse exited");
        };
    } 
}