using Godot;
using System;

public partial class WorldManager : Node {
	public static WorldManager Instance { get; private set; }
	
	[Export] private PackedScene packedServerData;
	[Export] public int BlockSize { get; private set; } = 100;

	[Signal]
	public delegate void CreatedServerWorldManagerEventHandler();
	
	public ServerData ServerData { get; private set; }

	public override void _Ready() {
		Instance = this;
	}
	
	private void OnStartedServer() {
		ServerData = packedServerData.Instantiate<ServerData>();
		AddChild(ServerData);
		EmitSignal(SignalName.CreatedServerWorldManager);
	}
	
}
