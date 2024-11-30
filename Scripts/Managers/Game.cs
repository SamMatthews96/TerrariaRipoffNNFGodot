using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class Game : Node {
    [Export] public Region Region { get; private set; }
    [Export] public Node BlockParent { get; private set; }
    [Export] public Node PlayerParent { get; private set; }
    [Export] public PackedScene HostManagerScene { get; private set; }


    public const int BlockSize = 32;

    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }
    
    public Host.Host Host { get; private set; }

    public bool IsHost => Multiplayer.GetUniqueId() == Manager.MultiplayerHostId;

    public event Action<PlayerInfo, int> PlayerJoined;

    public void Initialize(PlayerInfo playerInfo, Dictionary world) {
        if (IsHost) {
            Width = (int)world["Width"];
            Height = (int)world["Height"];
            Host = HostManagerScene.Instantiate<Host.Host>();
            AddChild(Host);
            Host.Initialize(world);
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