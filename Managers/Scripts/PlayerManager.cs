using Godot;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class PlayerManager : Node {
    [Export] private PackedScene packedPlayer;
    private PlayerInfo _playerInfo;
    
    [Signal]
    public delegate void LocalPlayerSpawnedEventHandler(int x, int y);

    public void Initialize(PlayerInfo playerInfo) {
        _playerInfo = playerInfo;
    }

    private void OnWorldManagerInitialized() {
        int peerId = Multiplayer.GetUniqueId();
        IntVector spawnPosition = WorldManager.Instance.GetPlayerSpawnPosition();
        RpcId(MultiplayerManager.HOST_ID, nameof(CreatePlayerOnServer),
            peerId, spawnPosition.X, spawnPosition.Y);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CreatePlayerOnServer(int peerId, int xSpawnCoords, int ySpawnCoords) {
        Player newPlayer = packedPlayer.Instantiate<Player>();
        newPlayer.Name = new StringName(peerId.ToString());
        Vector2 position = new(
            xSpawnCoords * BlockManager.BLOCK_SIZE, ySpawnCoords * BlockManager.BLOCK_SIZE);
        newPlayer.Position = position;

        AddChild(newPlayer);
        RpcId(peerId, nameof(EmitLocalPlayerSpawned), xSpawnCoords, ySpawnCoords);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void EmitLocalPlayerSpawned(int x, int y) {
        EmitSignal(SignalName.LocalPlayerSpawned, x, y);
    }
}