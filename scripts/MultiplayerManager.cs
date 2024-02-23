using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class MultiplayerManager : Node {
	[Export] private int port = 8910;
	[Export] private string address = "127.0.0.1";
	private ENetMultiplayerPeer peer;

	[Signal]
	public delegate void HostedGameEventHandler();

	[Signal]
	public delegate void NewPlayerJoinedEventHandler(long playerId);

	public override void _Ready() {
		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
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
		EmitSignal(SignalName.HostedGame);
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

	private void PeerConnected(long id) {
		if (Multiplayer.IsServer()) {
			RpcId(id, nameof(JoinGame));
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void JoinGame() {
		foreach (int currentPeer in Multiplayer.GetPeers()) {
			AddPlayer(currentPeer);
		}
		Rpc(nameof(AddPlayer), Multiplayer.GetUniqueId());
	}

	private void PeerDisconnected(long id) {
		// Print("Player Disconnected");
	}

	private void ConnectedToServer() {
		// Print("Connected to Server");
	}

	private void ConnectionFailed() {
		// Print("Disconnected from Server");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void AddPlayer(long playerId) {
		EmitSignal(SignalName.NewPlayerJoined, playerId);
	}
}
