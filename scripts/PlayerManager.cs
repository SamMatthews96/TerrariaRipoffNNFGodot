using Godot;
using System.Collections.Generic;
using TerrariaRipoffNNF.scripts;

public partial class PlayerManager : Node {
    public static PlayerManager Instance { get; private set; }

    [Export] private PackedScene packedPlayer;

    [Signal]
    public delegate void CreatedPlayerOnServerEventHandler(int xSpawnCoords, int ySpawnCoords);

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

        int xSpawnCoords = WorldManager.Instance.ServerData.SpawnX;
        int ySpawnCoords = WorldManager.Instance.ServerData.SpawnY;
        GD.Print(ySpawnCoords);

        AddChild(newPlayer);
        Rpc(nameof(OnPlayerCreatedOnServer), xSpawnCoords, ySpawnCoords);
    }

    [Rpc(CallLocal = true)]
    private void OnPlayerCreatedOnServer(int xSpawnCoords, int ySpawnCoords) {
        EmitSignal(SignalName.CreatedPlayerOnServer, xSpawnCoords, ySpawnCoords);
    }
}