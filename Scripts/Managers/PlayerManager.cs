using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node2D {
    [Export] private World _world;
    private Player _localPlayer;
    private List<Player> _players = new();
    private Dictionary _playerData;

    public event Action<Player> LocalPlayerSpawned;
    public event Action<Player> PlayerSpawnedOnServer;
    
    public void SpawnHostPlayer(Dictionary playerData) {
        int peerId = Multiplayer.GetUniqueId();
        Player player = Player.Create(peerId, new Vector2I(4, 14));
        player.InitAsLocal(_world.Game, playerData);
        AddChild(player, true);
        _localPlayer = player;
        _players.Add(player);
        LocalPlayerSpawned?.Invoke(player);
        PlayerSpawnedOnServer?.Invoke(player);
    }

    public void ClientSpawnPlayers(Dictionary playerData) {
        _playerData = playerData;
        RpcId(1, nameof(RpcHostAddPlayer));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostAddPlayer() {
        int peerId = Multiplayer.GetRemoteSenderId();
        RpcId(peerId, nameof(RpcNewClientAddExistingPlayers));
        Rpc(nameof(RpcAllAddNewPlayer), peerId);
    }

    [Rpc]
    private void RpcNewClientAddExistingPlayers() {
        foreach (int peerId in Multiplayer.GetPeers()) {
            if (peerId == Multiplayer.GetUniqueId()) continue;
            Player player = Player.Create(peerId, new Vector2I(4, 14));
            AddChild(player, true);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllAddNewPlayer(int peerId) {
        Player player = Player.Create(peerId, new Vector2I(4, 14));
        AddChild(player, true);
        _players.Add(player);
        if (peerId == Multiplayer.GetUniqueId()) {
            _localPlayer = player;
            player.InitAsLocal(_world.Game, _playerData);
        } else {
            _localPlayer.AddPeerToSynchronizer(peerId);
        }

        if (Multiplayer.IsServer()) {
            PlayerSpawnedOnServer?.Invoke(player);
        }
    }
}