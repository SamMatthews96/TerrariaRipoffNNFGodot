using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Game : Node {
    [Export] public Region Region { get; private set; }
    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }
    [Export] public Interface Interface { get; private set; }

    public const int BlockSize = 32;

    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }

    public Host Host { get; private set; }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.HostId;

    public event Action<PlayerInfo, int> PlayerConnected;

    public void Initialize(PlayerInfo playerInfo, Dictionary world) {
        if (IsHost) {
            Width = (int)world["Width"];
            Height = (int)world["Height"];
            Host = HostManagerScene.Instantiate<Host>();
            AddChild(Host);
            Host.Initialize(world);
        }

        RpcId(Manager.HostId, nameof(ServerHandleNewClient),
            playerInfo.Serialize(), Multiplayer.GetUniqueId());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerHandleNewClient(Dictionary playerDictionary, int peerId) {
        PlayerInfo playerInfo = PlayerInfo.FromDict(playerDictionary);
        PlayerConnected?.Invoke(playerInfo, peerId);
    }
}