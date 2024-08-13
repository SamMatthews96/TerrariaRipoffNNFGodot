// using Godot;
// using TerrariaRipoffNNF.Resources.Scripts;
// using TerrariaRipoffNNF.UI.Scripts;
// using static Godot.GD;
//
// namespace TerrariaRipoffNNF.Managers.Scripts;
//
// public partial class MultiplayerManager : Node {
//     [Export] private int port = 8910;
//     [Export] private string address = "127.0.0.1";
//     private ENetMultiplayerPeer peer;
//     public const int HostId = 1;
//
//     [Signal]
//     public delegate void ConnectedToServerEventHandler(PlayerInfo playerInfo);
//
//     public static MultiplayerManager Instance { get; private set; }
//     
//     public void StartHost() {
//         peer = new ENetMultiplayerPeer();
//         Error error = peer.CreateServer(port);
//         if (error != Error.Ok) {
//             Print("error cannot host! :" + error);
//             return;
//         }
//
//         peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
//         Multiplayer.MultiplayerPeer = peer;
//     }
//
//     public void StartClient(string ip, PlayerInfo playerInfo) {
//         //Multiplayer.PeerConnected += id => EmitSignal(SignalName.PeerConnected, id);
//         //Multiplayer.PeerDisconnected += id => EmitSignal(SignalName.PeerDisconnected, id);
//         //Multiplayer.ConnectionFailed += () => EmitSignal(SignalName.ConnectionFailed);
//         Multiplayer.ConnectedToServer += () => EmitSignal(SignalName.ConnectedToServer, playerInfo);
//
//         peer = new ENetMultiplayerPeer();
//         Error error = peer.CreateClient(ip, port);
//         if (error != Error.Ok) {
//             Print("error cannot join! :" + error);
//             return;
//         }
//
//         peer.Host.Compress(ENetConnection.CompressionMode.RangeCoder);
//         Multiplayer.MultiplayerPeer = peer;
//     }
// }