using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;

    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }

    [Export] public Region Region { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public InputManager InputManager { get; private set; }
    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }

    public Vector2 DefaultSpawnPosition { get; private set; }
    private WorldManager _worldManager;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    public WorldManager WorldManager {
        get => _worldManager ?? throw new Exception("[20241205.2011.1] Host not instantiated");
        private set => _worldManager = value;
    }

    public event Action<Dictionary> WorldCreated;

    public static Game Create() {
        return Data.PackedScenes.Game.Instantiate<Game>();
    }

    public override void _Ready() {
        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        CreateWorld(worldData);
        HostCreatePlayer(playerData, SceneManager.HostId);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        CreateWorld(worldData);
        HostCreatePlayer(playerData, SceneManager.HostId);
    }

    public void InitAsClient(Dictionary playerData) {
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);

        Multiplayer.ConnectedToServer += () => {
            RpcId(SceneManager.HostId, nameof(HostCreatePlayer),
                playerData, Multiplayer.GetUniqueId());
        };
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.InitAsLocal(this);
    }

    private void CreateWorld(Dictionary worldData) {
        Width = (int)worldData["Width"];
        Height = (int)worldData["Height"];
        WorldManager = WorldManager.Create();
        WorldManager.PickupManager.SetGame(this);
        WorldManager.BlockManager.SetGame(this);
        AddChild(WorldManager);

        DefaultSpawnPosition = new Vector2(
            (float)worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        WorldCreated?.Invoke(worldData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostCreatePlayer(Dictionary playerDictionary, int peerId) {
        Player player = Player.Create(peerId, playerDictionary);
        player.InitAsHost(this);
        PlayerParent.AddChild(player, true);
    }


    public bool IsInBounds(IntVector intVector) {
        return intVector.X >= 0 && intVector.X < Width && intVector.Y >= 0 && intVector.Y < Height;
    }
}