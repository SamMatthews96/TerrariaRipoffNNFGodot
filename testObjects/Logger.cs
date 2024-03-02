using Godot;

namespace TerrariaRipoffNNF.testObjects; 

public partial class Logger : Node {
	private void OnStartedServer() {
		int playerId = Multiplayer.MultiplayerPeer.GetUniqueId();
		GD.Print("hosted game: " + playerId);
	}

	private void OnConnectedToServer() {
		int playerId = Multiplayer.MultiplayerPeer.GetUniqueId();
		GD.Print("joined game: I am " + playerId);
	}

	private void OnPeerConnected(int playerId) {
		int thisPlayerId = Multiplayer.MultiplayerPeer.GetUniqueId();
		GD.Print(thisPlayerId + " joined by " + playerId);
	}

	private void OnServerPeerConnected(int playerId) {
		GD.Print("Server joined by " + playerId);
	}

	private void OnCallEvent() {
	}
	
}