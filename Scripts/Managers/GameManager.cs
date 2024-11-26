using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class GameManager : Node {
    [Export] public Region Region { get; private set; }
    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }


    public const int BlockSize = 32;
    public static GameManager Instance { get; private set; }

    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.MultiplayerHostId;

    public event Action<PlayerInfo, int> PlayerJoined;

    public void Initialize(PlayerInfo playerInfo, Dictionary world) {
        if (Instance is not null) {
            throw new Exception("[20240808.1730.1] GameManager already instantiated");
        }

        Instance = this;
        if (IsHost) {
            Width = (int)world["Width"];
            Height = (int)world["Height"];
            HostManager hostManager = HostManagerScene.Instantiate<HostManager>();
            AddChild(hostManager);
            hostManager.Initialize(world);
        }

        RpcId(Manager.MultiplayerHostId, nameof(ServerHandleNewClient),
            playerInfo.Serialize(), Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(Dictionary playerDictionary, int peerId) {
        PlayerInfo playerInfo = PlayerInfo.FromDict(playerDictionary);
        PlayerJoined?.Invoke(playerInfo, peerId);
    }
}