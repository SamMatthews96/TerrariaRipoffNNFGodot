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

    public static PlayerManager Instance { get; private set; }
    
    public override void _EnterTree() {
        Instance = this;
    }
    
    public void Initialize(PlayerInfo playerInfo) {
        _playerInfo = playerInfo;
        WorldManager.Instance.Initialized += OnWorldManagerInitialized;
    }

    private void OnWorldManagerInitialized() {
        int peerId = Multiplayer.GetUniqueId();
        RpcId(MultiplayerManager.HOST_ID, nameof(CreatePlayerOnServer), peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void CreatePlayerOnServer(int peerId) {
        GD.Print("before instantiate player");
        GD.Print(Multiplayer.GetUniqueId());
        Player newPlayer = packedPlayer.Instantiate<Player>();
        GD.Print("instantiated player");
        newPlayer.Name = new StringName(peerId.ToString());

        IntVector spawnPosition = WorldManager.Instance.GetPlayerSpawnPosition();      
        newPlayer.Position = new Vector2(spawnPosition.X * BlockManager.BLOCK_SIZE, 
            spawnPosition.Y * BlockManager.BLOCK_SIZE);
        AddChild(newPlayer);
        RpcId(peerId, nameof(EmitLocalPlayerSpawned), spawnPosition.X, spawnPosition.Y);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void EmitLocalPlayerSpawned(int x, int y) {
        EmitSignal(SignalName.LocalPlayerSpawned, x, y);
    }
}