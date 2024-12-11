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
    [Export] public Interface Interface { get; private set; }

    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }

    private Host _host;

    public Host Host {
        get => _host ?? throw new Exception("[20241205.2011.1] Host not instantiated");
        private set => _host = value;
    }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.HostId;

    public event Action<Dictionary> LaunchedGameAsHost;
    public event Action<Dictionary, int> PlayerConnected;

    public override void _Ready() {
        Manager.Instance.LaunchedGameAsHost += OnManagerLaunchedGameAsHost;
        Manager.Instance.JoinedGame += OnManagerJoinedGame;
    }

    private void OnManagerLaunchedGameAsHost(Dictionary world) {
        Width = (int)world["Width"];
        Height = (int)world["Height"];
        Host = HostManagerScene.Instantiate<Host>();
        AddChild(Host);
        LaunchedGameAsHost?.Invoke(world);
    }

    private void OnManagerJoinedGame(Dictionary playerInfo) {
        RpcId(Manager.HostId, nameof(ServerHandleNewClient),
            playerInfo, Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(Dictionary playerDictionary, int peerId) {
        PlayerConnected?.Invoke(playerDictionary, peerId);
    }
}