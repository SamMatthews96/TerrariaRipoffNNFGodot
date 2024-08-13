using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class GameManager : Node {
    public static GameManager Instance { get; private set; }
    private PlayerInfo _localPlayerInfo;

    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }
    [Export] public Region Region { get; private set; }
    [Export] public Node BlockParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.MultiplayerHostId;

    [Signal] public delegate void PlayerJoinedEventHandler(string playerUniqueName);

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240808.1730.1] GameManager already instantiated");
        }

        Instance = this;
    }

    public void Initialize(PlayerInfo playerInfo, Dictionary world) {
        if (IsHost) {
            Width = (int)world["Width"];
            Height = (int)world["Height"];
            HostManager hostManager = HostManagerScene.Instantiate<HostManager>();
            AddChild(hostManager);
            hostManager.Initialize(world);
        }

        _localPlayerInfo = playerInfo;
        RpcId(Manager.MultiplayerHostId, nameof(ServerHandleNewClient),
            _localPlayerInfo.UniqueName);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(string playerUniqueName) {
        EmitSignal(SignalName.PlayerJoined, playerUniqueName);
    }
}