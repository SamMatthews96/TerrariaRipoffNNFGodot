using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class NetworkTest : Node {
    private const int Port = 9999;
    private const string Address = "127.0.0.1";

    [Export] private PackedScene _networkTestScene;
    [Export] private Button _joinButton;
    [Export] private Button _hostButton;
    
    public event Action ClientStarted;
    public event Action HostStarted;
    
    public override void _Ready() {
        _hostButton.Pressed += OnHostPressed;
        _joinButton.Pressed += OnJoinPressed;
        TreeExiting += () => {
            _hostButton.Pressed -= OnHostPressed;
            _joinButton.Pressed -= OnJoinPressed;
        };
    }
    

    private void OnHostPressed() {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(Port, 2);
        if (error != Error.Ok) {
            GD.PrintErr($"Failed to create server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Server started on port " + Port);
        HostStarted?.Invoke();
    }

    private void OnJoinPressed() {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(Address, Port);
        if (error != Error.Ok) {
            GD.PrintErr($"Failed to connect to server: {error}");
            return;
        }
        Multiplayer.MultiplayerPeer = peer;
        GD.Print($"Connecting to {Address}:{Port}");
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        TreeExiting += () => {
            Multiplayer.ConnectedToServer -= OnConnectedToServer;
        };
    }

    private void OnConnectedToServer() {
        ClientStarted?.Invoke();
    }
}