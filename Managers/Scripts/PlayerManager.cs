using Godot;
using TerrariaRipoffNNF.Scenes.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class PlayerManager : Node {
    [Export] private PackedScene packedPlayer;

    [Signal]
    public delegate void CreatedLocalPlayerOnServerEventHandler();

    public static PlayerManager Instance { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    private void OnWorldCreated(int spawnX, int spawnY) {
        GD.Print("CreatePlayerOnServer");
        // CreatePlayerOnServer(GameManager.HOST_ID, x,y);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void CreatePlayerOnServer(int peerId, int xSpawnCoords, int ySpawnCoords) {
        Player newPlayer = packedPlayer.Instantiate<Player>();
        newPlayer.Name = new StringName(peerId.ToString());
        newPlayer.Position = new Vector2(
            xSpawnCoords * WorldManager.BLOCK_SIZE, ySpawnCoords* WorldManager.BLOCK_SIZE);

        AddChild(newPlayer);
        RpcId(peerId, nameof(OnPlayerCreatedOnServer));
    }

    [Rpc(CallLocal = true)]
    private void OnPlayerCreatedOnServer() {
        EmitSignal(SignalName.CreatedLocalPlayerOnServer);
    }
}