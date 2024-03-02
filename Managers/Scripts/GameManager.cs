using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class GameManager : Node {
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

    private void OnEnteredWorldHost() {
        Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
        
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port);
        if (error != Error.Ok) {
            Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
        EmitSignal(SignalName.StartedServer);
        Print("StartedServer");
    }

    private void OnEnteredWorldClient() {
        Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
        Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectedToServer);
        Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);
        
        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address, port);
        if (error != Error.Ok) {
            Print("error cannot join! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }

    private void OnEnteredWorldSinglePlayer() {
        Print("not implemented");
    }
}