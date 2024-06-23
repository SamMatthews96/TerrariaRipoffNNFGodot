using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.UI.Scripts;
using static Godot.GD;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class MultiplayerManager : Node {
    [Export] private int port = 8910;
    [Export] private string address = "127.0.0.1";
    private ENetMultiplayerPeer peer;
    public const int HOST_ID = 1;

    [Signal]
    public delegate void ConnectedToServerEventHandler(PlayerInfo playerInfo);

    public static MultiplayerManager Instance { get; private set; }
   
    public override void _EnterTree() {
        Instance = this;
    }
    
    public override void _Ready() {
        MainMenuScene.Instance.WorldLoadedHostMode += OnWorldLoadedHostMode;
        MainMenuScene.Instance.JoinGameButtonDown += OnJoinGameButtonDown;
    }
    
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