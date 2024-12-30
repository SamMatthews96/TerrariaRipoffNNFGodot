using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {

    public const int BlockSize = 32;
    [Export] public Region Region { get; private set; }
    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public InputManager InputManager { get; private set; }
    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }

    public event Action<Dictionary> LaunchedGameAsHost;
    
    public Vector2 DefaultSpawnPosition { get; private set; }
    private Host _host;

    public Host Host {
        get => _host ?? throw new Exception("[20241205.2011.1] Host not instantiated");
        private set => _host = value;
    }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.HostId;

    public override void _Ready() {
        Manager.Instance.JoinedGame += OnManagerJoinedGame;
        Manager.Instance.LaunchedGameAsHost += OnManagerLaunchedGameAsHost;
    }

    public override void _ExitTree() {
        Manager.Instance.JoinedGame -= OnManagerJoinedGame;
        Manager.Instance.LaunchedGameAsHost -= OnManagerLaunchedGameAsHost;
    }

    public static Game Create() {
        Game game = Data.PackedScenes.Game.Instantiate<Game>();
        return game;
    }
    
    
    
    private void OnManagerLaunchedGameAsHost(Dictionary world) {
        Width = (int)world["Width"];
        Height = (int)world["Height"];
        Host = HostManagerScene.Instantiate<Host>();
        AddChild(Host);

        DefaultSpawnPosition = new Vector2(
            (float)world["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)world["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        
        LaunchedGameAsHost?.Invoke(world);
    }

    private void OnManagerJoinedGame(Dictionary playerInfo) {
        RpcId(Manager.HostId, nameof(ServerHandleNewClient),
            playerInfo, Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(Dictionary playerDictionary, int peerId) {
        Player player = Player.Create(peerId, playerDictionary);
        PlayerParent.AddChild(player, true);
    }
}