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
    private WorldObjectManager _worldObjectManager;

    private MultiplayerHost _multiplayerHost;
    private MultiplayerClient _multiplayerClient;

    private Dictionary _playerData;

    public WorldObjectManager WorldObjectManager {
        get => _worldObjectManager ?? throw new Exception("[20241205.2011.1] Host not instantiated");
        private set => _worldObjectManager = value;
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
        CreateWorld(worldData, playerData);
    }

    public void InitAsHost(Dictionary worldData, Dictionary playerData) {
        _multiplayerHost = new MultiplayerHost();
        AddChild(_multiplayerHost);

        CreateWorld(worldData, playerData);
    }

    public void InitAsClient(Dictionary playerData) {
        _playerData = FileManager.LoadPlayer(playerData);
        _multiplayerClient = new MultiplayerClient();
        AddChild(_multiplayerClient);

        Multiplayer.ConnectedToServer += () => {
            RpcId(SceneManager.HostId,
                nameof(HostCreatePlayer),
                Multiplayer.GetUniqueId());
        };
    }

    private void OnWorldLoaded() {
        HostCreatePlayer(SceneManager.HostId);
        _playerData = null;
        GameLoaded?.Invoke();
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.InitAsLocal(this, _playerData);
        _playerData = null;
    }

    private void CreateWorld(Dictionary worldData, Dictionary playerData) {
        Width = (int)worldData["Width"];
        Height = (int)worldData["Height"];
        WorldObjectManager = WorldObjectManager.Create();
        WorldObjectManager.SetGame(this, worldData, playerData);
        AddChild(WorldObjectManager);

        DefaultSpawnPosition = new IntVector(
            worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsInt32(),
            worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsInt32());

        _playerData = FileManager.LoadPlayer(playerData);
        WorldObjectManager.WorldLoaded += OnWorldLoaded;

        TreeExiting += () => { WorldObjectManager.WorldLoaded -= OnWorldLoaded; };
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostCreatePlayer(int peerId) {
        Player player = Player.Create(peerId, DefaultSpawnPosition);
        player.InitAsHost(this);
        PlayerParent.AddChild(player, true);
    }

    public bool IsInBounds(IntVector intVector) {
        return intVector.X >= 0 && intVector.X < Width && intVector.Y >= 0 && intVector.Y < Height;
    }
}