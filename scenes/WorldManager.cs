using Godot;
using System;

public partial class WorldManager : Node {
	[Export] private PackedScene world;

	private void OnStartedServer() {
		CreateWorld();
		PlayerManager.Instance.SpawnPlayer(1);
	}

	private void OnConnectedToServer() {
		int playerId = Multiplayer.GetUniqueId();
		CreateWorld();
		PlayerManager.Instance.SpawnPlayer(playerId);
	}

	private void OnPeerConnected(int playerId) {
		PlayerManager.Instance.SpawnPlayer(playerId);
	}

	private void CreateWorld() {
		Node newWorld = world.Instantiate();
		AddChild(newWorld);
	}
}
