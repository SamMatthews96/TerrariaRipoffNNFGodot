using Godot;
using TerrariaRipoffNNF.Scenes.Scripts;
using static Godot.GD;

namespace TerrariaRipoffNNF.GameManagers.Scripts;

public partial class GameManagerHost : GameManager {
    [Export] private int port = 8910;
    private ENetMultiplayerPeer peer;

    [Signal]
    public delegate void StartedServerEventHandler();

    [Signal]
    public delegate void PeerConnectedEventHandler(long playerId);

    [Signal]
    public delegate void PeerDisconnectedEventHandler(long playerId);
    
    [Export] public ServerData ServerData { get; private set; }

    public override void _Ready() {
        Instance = this;
        
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
    }
}