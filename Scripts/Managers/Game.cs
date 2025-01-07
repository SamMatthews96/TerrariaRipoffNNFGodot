using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;
    private Dictionary _worldData;
    private Dictionary _playerData;

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

    public bool IsHost => Multiplayer.GetUniqueId() == SceneManager.HostId;

    public event Action<Dictionary> WorldCreated;

    public static Game CreateSinglePlayer(Dictionary worldData, Dictionary playerData) {
        Game game = Data.PackedScenes.Game.Instantiate<Game>();
        game._worldData = worldData;
        game._playerData = playerData;
        return game;
    }

    public override void _Ready() {
        if (_worldData is not null) {
            CreateWorld();
        }

        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
        
        RpcId(SceneManager.HostId, nameof(HostCreatePlayer),
            _playerData, Multiplayer.GetUniqueId());
        // _worldData = null;
        // _playerData = null;
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.InitAsLocal(this);
    }

    private void CreateWorld() {
        Width = (int)_worldData["Width"];
        Height = (int)_worldData["Height"];
        WorldManager = WorldManager.Create();
        WorldManager.SetGame(this);
        AddChild(WorldManager);

        DefaultSpawnPosition = new Vector2(
            (float)_worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)_worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        WorldCreated?.Invoke(_worldData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostCreatePlayer(Dictionary playerDictionary, int peerId) {
        Player player = Player.Create(peerId, playerDictionary);
        player.InitAsHost();
        PlayerParent.AddChild(player, true);
    }


    public bool IsInBounds(IntVector intVector) {
        return intVector.X >= 0 && intVector.X < Width && intVector.Y >= 0 && intVector.Y < Height;
    }
}