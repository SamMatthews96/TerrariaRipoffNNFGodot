using Godot;
using System;

public partial class WorldManager : Node {
	[Export] private PackedScene world;
	
	private void OnStartedServer() {
		World newWorld = world.Instantiate<World>();
		AddChild(newWorld);
	}
	
}
