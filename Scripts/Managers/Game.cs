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

    public event Action GameLoaded;

    public IntVector DefaultSpawnPosition { get; private set; }
    private WorldManager _worldManager;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    public WorldManager WorldManager {
        get => _worldManager ?? throw new Exception("[20241205.2011.1] Host not instantiated");
        private set => _worldManager = value;
    }


    public static Game Create() {
        return Data.PackedScenes.Game.Instantiate<Game>();
    }

    public override void _Ready() {
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }
    
    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        CreateWorld(worldData);
        _playerData = playerData;
        WorldManager.BlockManager.WorldLoaded += OnWorldLoaded;
        TreeExiting += OnExitingSinglePlayer;
    }

    private void OnExitingSinglePlayer() {
        WorldManager.BlockManager.WorldLoaded -= OnWorldLoaded;
        TreeExiting -= OnExitingSinglePlayer;
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        CreateWorld(worldData);
        _playerData = playerData;
        WorldManager.BlockManager.WorldLoaded += OnWorldLoaded;
        TreeExiting += OnExitingHost;
    }

    private void OnExitingHost() {
        WorldManager.BlockManager.WorldLoaded -= OnWorldLoaded;
        TreeExiting -= OnExitingHost;
    }

    private void OnWorldLoaded() {
        HostCreatePlayer(_playerData, SceneManager.HostId);
        _playerData = null;
        GameLoaded?.Invoke();
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
        WorldManager.BlockManager.SetGame(this, worldData);
        WorldManager.PlaceableManager.SetGame(this, worldData);
        AddChild(WorldManager);

        DefaultSpawnPosition = new IntVector(
            worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsInt32(),
            worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsInt32());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostCreatePlayer(Dictionary playerDictionary, int peerId) {
        Player player = Player.Create(peerId, DefaultSpawnPosition, this);
        player.InitAsHost(this);
        PlayerParent.AddChild(player, true);
    }

    public bool IsInBounds(IntVector intVector) {
        return intVector.X >= 0 && intVector.X < Width && intVector.Y >= 0 && intVector.Y < Height;
    }
}