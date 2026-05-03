using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node2D {
    [Export] private World _world;
    private Player _localPlayer;
    private Dictionary _playerData;
    private Dictionary<long, Player> _players = new();

    public event Action<Player> LocalPlayerSpawned;
    public event Action<Player> PlayerSpawnedOnHost;

    public override void _Ready() {
        if (!_world.IsHost) return;

        _world.PropManager.HostPropPlaced += OnHostPropPlaced;
        _world.PropManager.HostPropDestroyed += OnHostPropDestroyed;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        TreeExiting += () => {
            _world.PropManager.HostPropPlaced -= OnHostPropPlaced;
            _world.PropManager.HostPropDestroyed -= OnHostPropDestroyed;
            Multiplayer.PeerDisconnected -= OnPeerDisconnected;
        };
    }

    private void OnHostPropPlaced(Prop prop, Vector2I coords) {
        if (!prop.Item.GetProperty<ItemProp>().HasProperty<PropStation>()) return;
        foreach (Player player in _players.Values) {
            int range = player.Crafting.CraftRange;
            if (_world.IsInOrthogonalRange(player.Coords, coords, range)) {
                player.Crafting.HostAddCraftingStation(prop);
            }
        }
    }

    private void OnHostPropDestroyed(Prop prop, Vector2I vector2I) {
        if (!prop.Item.GetProperty<ItemProp>().HasProperty<PropStation>()) return;
        foreach (Player player in _players.Values) {
            int range = player.Crafting.CraftRange;
            if (_world.IsInOrthogonalRange(player.Coords, vector2I, range)) {
                player.Crafting.HostRemoveCraftingStation(prop);
            }
        }
    }

    private void OnPeerDisconnected(long id) {
        Rpc(nameof(RpcAllDeletePlayer), id);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDeletePlayer(long id) {
        Player player = _players[id];
        _players.Remove(id);
        player.QueueFree();
    }

    public void SpawnHostPlayer(Dictionary playerData) {
        int peerId = Multiplayer.GetUniqueId();
        _localPlayer = CreateNewPlayer(peerId, playerData);

        LocalPlayerSpawned?.Invoke(_localPlayer);
        PlayerSpawnedOnHost?.Invoke(_localPlayer);
    }

    public void SpawnPlayersOnClient(Dictionary playerData) {
        int[] peers = Multiplayer.GetPeers();
        foreach (int peer in peers) {
            CreateNewPlayer(peer);
        }

        int peerId = Multiplayer.GetUniqueId();
        _localPlayer = CreateNewPlayer(peerId, playerData);
        LocalPlayerSpawned?.Invoke(_localPlayer);
        RpcId(1, nameof(RpcHostHandleNewPeer), playerData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostHandleNewPeer(Dictionary playerData) {
        int senderId = Multiplayer.GetRemoteSenderId();

        Player remotePlayer = CreateNewPlayer(senderId, playerData);
        _localPlayer.AddPeerToSynchronizer(senderId);
        PlayerSpawnedOnHost?.Invoke(remotePlayer);

        int[] peers = Multiplayer.GetPeers();
        foreach (int peerId in peers) {
            if (peerId == senderId) continue;
            RpcId(peerId, nameof(RpcSpawnNewPlayer), senderId);
        }
    }

    [Rpc]
    private void RpcSpawnNewPlayer(int peerId) {
        CreateNewPlayer(peerId);
        _localPlayer.AddPeerToSynchronizer(peerId);
    }

    private Player CreateNewPlayer(int peerId, Dictionary playerData = null) {
        Player player = Player.Create(_world, peerId, playerData);
        AddChild(player, true);
        _players.Add(peerId, player);
        return player;
    }
}