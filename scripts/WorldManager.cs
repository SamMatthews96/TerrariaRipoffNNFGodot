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
	
	/*
	 * player.LocalPlayerEnteredLocation += (x,y,x,y) => {
	 *		delete active blocks that are out of range
	 *		get server info of active blocks within range
	 * }
	 */

	private void OnCreatedLocalPlayer(int xSpawnCoords, int ySpawnCoords) {
		Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
		int peerId = Multiplayer.GetUniqueId();

		// spawn active blocks from nearby
		RpcId(MultiplayerManager.HOST_ID, nameof(SendNewBlocksToPeer),
			peerId,xSpawnCoords,ySpawnCoords);
	}

	private void OnLocalPlayerMoved(int xCoords, int yCoords, int prevXCoords, int prevYCoords) {
		int peerId = Multiplayer.GetUniqueId();
		// spawn + despawn active blocks from nearby
		RpcId(MultiplayerManager.HOST_ID, nameof(SendNewBlocksToPeer),
			peerId,xCoords,yCoords,prevXCoords,prevYCoords);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void SendNewBlocksToPeer(int peerId,
		int xCoords, int yCoords, int prevXCoords = int.MaxValue, int prevYCoords = int.MaxValue) {
		// get the new blocks
		

		RpcId(peerId, nameof(ReceiveNewBlocksFromServer));
	}

	[Rpc(CallLocal = true)]
	private void ReceiveNewBlocksFromServer() {
		
	}
	
}
