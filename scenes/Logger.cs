using Godot;
using System;

public partial class Logger : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnHostedGame() {
		GD.Print("hosted game: " + Multiplayer.MultiplayerPeer.GetUniqueId());
	}

	private void OnNewPlayerJoined(long playerId) {
		GD.Print("new player joined: " + Multiplayer.MultiplayerPeer.GetUniqueId());
		GD.Print("their id is: " + playerId);
	}
}
