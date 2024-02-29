using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class MultiplayerManager : Node {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;
    public const int HOST_ID = 1;

    [Signal]
    public delegate void StartedServerEventHandler();

    [Signal]
    public delegate void PeerConnectedEventHandler(long playerId);

    [Signal]
    public delegate void PeerDisconnectedEventHandler(long playerId);

    [Signal]
    public delegate void ConnectedToServerEventHandler();

    [Signal]
    public delegate void ConnectionFailedEventHandler();

    public override void _Ready() {
        Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
        Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectedToServer);
        Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);
    }

    private void OnLoginScreenHostButtonDown() {
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port);
        if (error != Error.Ok) {
            Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        EmitSignal(SignalName.StartedServer);
    }

    private void OnLoginScreenJoinButtonDown() {
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address, port);
        if (error != Error.Ok) {
            Print("error cannot join! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }
}