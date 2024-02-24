using Godot;
using System.Collections.Generic;
using TerrariaRipoffNNF.scripts;

public partial class PlayerManager : Node {
	[Export] private PackedScene packedPlayer;
	
	public static PlayerManager Instance { get; set; }

	public override void _Ready() {
		Instance = this;
	}
	
	public void SpawnPlayer(int playerId) {
		Player newPlayer = packedPlayer.Instantiate<Player>();
		newPlayer.Name = playerId.ToString();
		AddChild(newPlayer);
	}
	
	
}
