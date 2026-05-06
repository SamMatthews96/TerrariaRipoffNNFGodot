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

        _world.StationManager.StationCreated += OnStationCreated;
        _world.StationManager.StationDestroyed += OnStationDestroyed;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        TreeExiting += () => {
            Multiplayer.PeerDisconnected -= OnPeerDisconnected;
        };
    }

    private void OnStationCreated(Vector2I coords, StationType type) {
        foreach (Player player in _players.Values) {
            int range = player.Crafting.CraftRange;
            if (_world.IsInOrthogonalRange(player.Coords, coords, range)) {
                player.Crafting.HostAddCraftingStation(coords, type);
            }
        }
    }

    private void OnStationDestroyed(Vector2I coords, StationType type) {
        foreach (Player player in _players.Values) {
            int range = player.Crafting.CraftRange;
            if (_world.IsInOrthogonalRange(player.Coords, coords, range)) {
                player.Crafting.HostRemoveCraftingStation(coords, type);
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