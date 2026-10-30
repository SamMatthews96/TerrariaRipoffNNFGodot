using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node2D {
    [Export] private World _world;
    private Player _localPlayer;
    private Dictionary<long, Player> _players = new();

    public event Action<Player> LocalPlayerSpawned;
    public event Action<Player> HostPlayerSpawned;

    public int GetPlayerId(Player player) {
        return player.PeerId;
    }
    
    public Player GetPlayer(int id) {
        return _players[id];
    }

    public override void _Ready() {
        if (_world.IsHost) {
            Multiplayer.PeerDisconnected += OnPeerDisconnected;
            TreeExiting += () => {
                Multiplayer.PeerDisconnected -= OnPeerDisconnected;
            };
        }

        SpawnLocalPlayer(_world.LocalPlayerData);
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

    private void SpawnLocalPlayer(Dictionary playerData) {
        if (_world.IsHost) {
            int peerId = Multiplayer.GetUniqueId();
            _localPlayer = CreateNewPlayer(peerId, playerData);

            LocalPlayerSpawned?.Invoke(_localPlayer);
            HostPlayerSpawned?.Invoke(_localPlayer);
        } else {
            int[] peers = Multiplayer.GetPeers();
            foreach (int peer in peers) {
                CreateNewPlayer(peer);
            }

            int peerId = Multiplayer.GetUniqueId();
            _localPlayer = CreateNewPlayer(peerId, playerData);
            LocalPlayerSpawned?.Invoke(_localPlayer);
            this.RpcHost(nameof(RpcHostHandleNewPeer), playerData);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostHandleNewPeer(Dictionary playerData) {
        int senderId = Multiplayer.GetRemoteSenderId();

        Player remotePlayer = CreateNewPlayer(senderId, playerData);
        _localPlayer.AddPeerToSynchronizer(senderId);
        HostPlayerSpawned?.Invoke(remotePlayer);

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