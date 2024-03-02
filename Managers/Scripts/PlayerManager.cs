using Godot;
using TerrariaRipoffNNF.Scenes.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class PlayerManager : Node {
    [Export] private PackedScene packedPlayer;

    [Signal]
    public delegate void CreatedLocalPlayerEventHandler(int xSpawnCoords, int ySpawnCoords);

    public static PlayerManager Instance { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    private void OnStartedServer() {
        CreatePlayerOnServer(MultiplayerManager.HOST_ID);
    }

    private void OnConnectedToServer() {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(CreatePlayerOnServer), peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void CreatePlayerOnServer(int peerId) {
        Player newPlayer = packedPlayer.Instantiate<Player>();
        newPlayer.Name = new StringName(peerId.ToString());

        int xSpawnCoords = ServerData.Instance.SpawnX;
        int ySpawnCoords = ServerData.Instance.SpawnY;

        AddChild(newPlayer);
        RpcId(peerId, nameof(OnPlayerCreatedOnServer),
            xSpawnCoords, ySpawnCoords);
    }

    [Rpc(CallLocal = true)]
    private void OnPlayerCreatedOnServer(int xSpawnCoords, int ySpawnCoords) {
        EmitSignal(SignalName.CreatedLocalPlayer, xSpawnCoords, ySpawnCoords);
    }
}