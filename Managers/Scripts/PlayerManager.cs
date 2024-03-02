using Godot;
using TerrariaRipoffNNF.Scenes.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class PlayerManager : Node {
    [Export] private PackedScene packedPlayer;

    [Signal]
    public delegate void CreatedLocalPlayerOnServerEventHandler(Vector2 position);

    public static PlayerManager Instance { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    private void OnWorldCreated(int spawnX, int spawnY) {
        GD.Print(spawnX,spawnY);
        int peerId = Multiplayer.GetUniqueId();
        RpcId(GameManager.HOST_ID, nameof(CreatePlayerOnServer),
            peerId, spawnX, spawnY);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CreatePlayerOnServer(int peerId, int xSpawnCoords, int ySpawnCoords) {
        Player newPlayer = packedPlayer.Instantiate<Player>();
        newPlayer.Name = new StringName(peerId.ToString());
        Vector2 position = new Vector2(
            xSpawnCoords * WorldManager.BLOCK_SIZE, ySpawnCoords * WorldManager.BLOCK_SIZE);
        newPlayer.Position = position;

        AddChild(newPlayer);
        RpcId(peerId, nameof(OnPlayerCreatedOnServer), position);
    }

    [Rpc(CallLocal = true)]
    private void OnPlayerCreatedOnServer(Vector2 position) {
        EmitSignal(SignalName.CreatedLocalPlayerOnServer, position);
    }
}