using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class MultiplayerClient : Node {
    private ENetMultiplayerPeer _peer;
    private int _port = 8910;
    private string _ip = "127.0.0.1";
   
    public override void _Ready() {
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(_ip, _port);
        if (error != Error.Ok) {
            throw new Exception("error cannot join! [20240808.1337.1] :" + error);
        }

        _peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = _peer;
    }
    
    public override void _ExitTree() {
        _peer.Close();
        Multiplayer.MultiplayerPeer = new OfflineMultiplayerPeer();
    }
}