using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    public const int BlockSize = 32;


    //@todo might be able to remove:
    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }

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

    
    public static Game Create() {
        return Data.PackedScenes.Game.Instantiate<Game>();
    }

    private bool _initialized;
    public void InitAsSinglePlayer(Dictionary worldData, Dictionary playerData) {
        if (_initialized) {
            throw new Exception("[20241205.2011.2] Game already initialized");
        }
        CreateWorld(worldData);
        CreatePlayer(playerData);
        _initialized = true;
    }
    

    private void CreateWorld(Dictionary worldData) {
        Width = (int)worldData["Width"];
        Height = (int)worldData["Height"];
        WorldManager = HostManagerScene.Instantiate<WorldManager>();
        AddChild(WorldManager);

        DefaultSpawnPosition = new Vector2(
            (float)worldData["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldData["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        WorldCreated?.Invoke(worldData);
    }

    private void CreatePlayer(Dictionary playerData) {
        RpcId(SceneManager.HostId, nameof(ServerHandleNewClient),
            playerData, Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(Dictionary playerDictionary, int peerId) {
        Player player = Player.Create(peerId, playerDictionary);
        //@todo do we need PlayerParent?
        PlayerParent.AddChild(player, true);
    }
}