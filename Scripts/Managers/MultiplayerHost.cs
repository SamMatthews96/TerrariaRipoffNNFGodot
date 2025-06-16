using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class MultiplayerHost : Node {
    private ENetMultiplayerPeer _peer;
    private int _port = 8910;

    public override void _Ready() {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(_port);
        if (error != Error.Ok) {
            throw new Exception("error cannot host! [20240808.1336.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }
}