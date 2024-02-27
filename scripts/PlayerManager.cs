using Godot;
using System.Collections.Generic;
using TerrariaRipoffNNF.scripts;

public partial class PlayerManager : Node {
	public static PlayerManager Instance { get; private set; }

	[Export] private PackedScene packedPlayer;

	[Signal]
	public delegate void CreatedPlayerOnServerEventHandler();


	public override void _Ready() {
		Instance = this;
	}

	private void OnConnectedToServer() {
		int playerId = Multiplayer.GetUniqueId();
		RpcId(1, nameof(CreatePlayerOnServer), playerId);
	}

	private void OnCreatedServerWorldManager() {
		CreatePlayerOnServer(1);
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	private void CreatePlayerOnServer(int peerId) {
		Player newPlayer = packedPlayer.Instantiate<Player>();
		newPlayer.Name = new StringName(peerId.ToString());
		
		int newX = WorldManager.Instance.ServerData.SpawnX;
		int newY = WorldManager.Instance.ServerData.SpawnY;
		int blockSize = WorldManager.Instance.BlockSize;
		
		// this will probably need to be done a lot, where to put it
		newPlayer.Position = new Vector2(newX * blockSize, newY * blockSize);
		AddChild(newPlayer);
		Rpc(nameof(OnPlayerCreatedOnServer));
		GD.Print("run rpc");
	}

	[Rpc(CallLocal = true)]
	private void OnPlayerCreatedOnServer() {
		GD.Print("OnPlayerCreatedOnServer");
		EmitSignal(SignalName.CreatedPlayerOnServer);
	}


}
