using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node2D {
    [Export] private World _world;
    private Player _localPlayer;
    private Dictionary _playerData;

    public event Action<Player> LocalPlayerSpawned;
    public event Action<Player> PlayerSpawnedOnServer;

    public void SpawnHostPlayer(Dictionary playerData) {
        int peerId = Multiplayer.GetUniqueId();
        Player player = Player.Create(
            _world, peerId, new Vector2I(4, 14), playerData);
        AddChild(player, true);
        _localPlayer = player;
        LocalPlayerSpawned?.Invoke(player);
        PlayerSpawnedOnServer?.Invoke(player);
    }

    public void SpawnPlayersForNewPeer(Dictionary playerData) {
        _playerData = playerData;
        RpcId(1, nameof(RpcHostSpawnPlayersForNewPeer), playerData);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostSpawnPlayersForNewPeer(Dictionary playerData) {
        int peerId = Multiplayer.GetRemoteSenderId();
        RpcId(peerId, nameof(RpcNewClientAddExistingPlayers));
        Rpc(nameof(RpcAllAddNewPlayer), peerId);

        Player player = Player.Create(
            _world, peerId, new Vector2I(4, 14), playerData);
        AddChild(player, true);
        _localPlayer.AddPeerToSynchronizer(peerId);
        PlayerSpawnedOnServer?.Invoke(player);
    }

    [Rpc]
    private void RpcNewClientAddExistingPlayers() {
        foreach (int peerId in Multiplayer.GetPeers()) {
            if (peerId == Multiplayer.GetUniqueId()) continue;
            Player player = Player.Create(_world, peerId, new Vector2I(4, 14));
            AddChild(player, true);
        }
    }

    [Rpc]
    private void RpcAllAddNewPlayer(int peerId) {
        Player player;
        bool isLocalPlayer = peerId == Multiplayer.GetUniqueId();
        if (isLocalPlayer) {
            player = Player.Create(_world, peerId, new Vector2I(4, 14), _playerData);
            AddChild(player, true);

            _localPlayer = player;
            LocalPlayerSpawned?.Invoke(player);
        } else {
            player = Player.Create(_world, peerId, new Vector2I(4, 14));
            AddChild(player, true);

            _localPlayer.AddPeerToSynchronizer(peerId);
        }
    }
}