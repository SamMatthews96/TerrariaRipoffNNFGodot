using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class MultiplayerMenu : Control {
    [Export] private Button _hostButton;
    [Export] private Button _joinButton;
    [Export] private Button _backButton;
    
    public event Action HostButtonDown;
    public event Action JoinButtonDown;
    public event Action BackButtonDown;
    
    public override void _Ready() {
        Hide();
        _hostButton.ButtonDown += OnHostButtonDown;
        _joinButton.ButtonDown += OnJoinButtonDown;
        _backButton.ButtonDown += OnBackButtonDown;
    }

    public override void _ExitTree() {
        _hostButton.ButtonDown -= OnHostButtonDown;
        _joinButton.ButtonDown -= OnJoinButtonDown;
        _backButton.ButtonDown -= OnBackButtonDown;
    }

    private void OnHostButtonDown() {
        Hide();
        HostButtonDown?.Invoke();
    }

    private void OnJoinButtonDown() {
        Hide();
        JoinButtonDown?.Invoke();
    }

    private void OnBackButtonDown() {
        Hide();
        BackButtonDown?.Invoke();
    }
}