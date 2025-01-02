using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class MultiplayerClient : Node {
    private ENetMultiplayerPeer _peer;
    private int _port = 8910;
    private string _ip;

    public event Action ConnectedToServer;
    
    public override void _Ready() {
        // need to pass ip to this class
        Multiplayer.ConnectedToServer += () => {
            ConnectedToServer?.Invoke();
        };

        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(_ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }

}