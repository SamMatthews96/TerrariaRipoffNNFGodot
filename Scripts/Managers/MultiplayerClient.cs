using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class MultiplayerClient : Node {
    private ENetMultiplayerPeer _peer;
    private int _port = 8910;
    private string _ip = "127.0.0.1";
    private Game _game;

    public MultiplayerClient(Game game) {
        _game = game;
    }

    public MultiplayerClient() { }

    public override void _Ready() {
        Multiplayer.ConnectedToServer += () => { };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(_ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;

        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameButtonDown;
    }

    private void OnExitGameButtonDown() {
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameButtonDown;
        _peer.Close();
        QueueFree();
    }
}