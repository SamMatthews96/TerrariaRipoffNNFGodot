using Godot;
using System;
using TerrariaRipoffNNF.scripts;

public partial class WorldManager : Node {
	public static WorldManager Instance { get; private set; }

	[Signal]
	public delegate void CreatedServerWorldManagerEventHandler();
	
	[Export] private PackedScene packedServerData;
	[Export] public int BlockSize { get; private set; } = 100;
	[Export] private int activeBlockViewDistance = 10;
	
	public ServerData ServerData { get; private set; }

	public override void _Ready() {
		Instance = this;
	}
	
	private void OnStartedServer() {
		ServerData = packedServerData.Instantiate<ServerData>();
		AddChild(ServerData);
		EmitSignal(SignalName.CreatedServerWorldManager);
	}

	private void OnCreatedLocalPlayer(int xSpawnCoords, int ySpawnCoords) {
		// spawn active blocks from nearby
		GD.Print("OnCreatedLocalPlayer");
		Player.LocalPlayer.LocalPlayerMoved +=
			(xCoords, yCoords, prevXCoords, prevYCoords) => {
				GD.Print("LocalPlayerMoved");
				// spawn / despawn active blocks from nearby
			};
	}
	
	
	/*
	 * player.LocalPlayerEnteredLocation += (x,y,x,y) => {
	 *		delete active blocks that are out of range
	 *		get server info of active blocks within range
	 *		
	 * }
	 * 
	 */
	
}
