using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.GameManagers.Scripts;

public partial class GameManagerClient : GameManager {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;

    [Signal]
    public delegate void PeerConnectedEventHandler(long playerId);

    [Signal]
    public delegate void PeerDisconnectedEventHandler(long playerId);

    [Signal]
    public delegate void ConnectedToServerEventHandler();

    [Signal]
    public delegate void ConnectionFailedEventHandler();
    
    

    public override void _Ready() {
        Instance = this;

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
        
        //get server data
        //load active blocks
        //create player
    }
}