using System.Threading.Tasks;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using static Godot.GD;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class GameManager : Node {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;
    public const int HOST_ID = 1;


    [Signal]
    public delegate void StartedGameEventHandler();

    [Signal]
    public delegate void PeerConnectedEventHandler(long playerId);

    [Signal]
    public delegate void PeerDisconnectedEventHandler(long playerId);

    [Signal]
    public delegate void ConnectedToServerEventHandler();

    [Signal]
    public delegate void ConnectionFailedEventHandler();
    
    [Signal]
    public delegate void LoadGameInitializedEventHandler(WorldBasicInfo worldBasicInfo);
    
    private void OnEnterWorldAsSingleButtonDown(WorldBasicInfo worldBasicInfo) {
        Task.Run(() => CreateWorldAsSingle(worldBasicInfo));
    }

    private void CreateWorldAsSingle(WorldBasicInfo worldBasicInfo) {
        EmitSignal(SignalName.LoadGameInitialized, worldBasicInfo);
    }

    private void CreateWorldAsHost(WorldBasicInfo worldBasicInfo) {
        EmitSignal(SignalName.LoadGameInitialized, worldBasicInfo);

        Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);

        peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port);
        if (error != Error.Ok) {
            Print("error cannot host! :" + error);
            return;
        }

        peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
        Multiplayer.MultiplayerPeer = peer;
    }

    private void OnEnterWorldAsHostButtonDown(WorldBasicInfo worldBasicInfo) {
        Task.Run(() => CreateWorldAsHost(worldBasicInfo));
    }

    private void OnEnterWorldAsClientButtonDown(string ip) {
        Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
        Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
        Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectedToServer);
        Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);

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