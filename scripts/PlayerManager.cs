using Godot;
using System.Collections.Generic;
using TerrariaRipoffNNF.scripts;

public partial class PlayerManager : Node {
	[Export] private PackedScene packedPlayer;
	[Export] private MultiplayerSpawner spawner;

	public override void _Ready() {
		int thisPlayerId = Multiplayer.GetUniqueId();
		GD.Print("PlayerManager " + thisPlayerId);
		spawner.SetMultiplayerAuthority(thisPlayerId);
	}

	private void OnConnectedToServer() {
		Player newPlayer = packedPlayer.Instantiate<Player>();
		int playerId = Multiplayer.GetUniqueId();
		newPlayer.Name = new StringName(playerId.ToString());
		AddChild(newPlayer);
	}

	private void OnStartedServer() {
		Player newPlayer = packedPlayer.Instantiate<Player>();
		newPlayer.Name = new StringName("1");
		AddChild(newPlayer);
	}
	
	
}
