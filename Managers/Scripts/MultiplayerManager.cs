using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using static Godot.GD;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class MultiplayerManager : Node {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;
    public const int HOST_ID = 1;

    [Signal]
    public delegate void ConnectedToServerEventHandler(PlayerInfo playerInfo);

    private void OnWorldLoadedHostMode() {
        //Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        //Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);

        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port);
        if (error != Error.Ok) {
            Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }

    private void OnJoinGameButtonDown(string ip, PlayerInfo playerInfo) {
        //Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        //Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
        Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectedToServer, playerInfo);
        //Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);

        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(ip, port);
        if (error != Error.Ok) {
            Print("error cannot join! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }
}