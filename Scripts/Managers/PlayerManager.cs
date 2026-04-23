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
        Player player = Player.Create(_world, peerId, playerData);
        AddChild(player, true);
        _localPlayer = player;
        LocalPlayerSpawned?.Invoke(player);
        PlayerSpawnedOnServer?.Invoke(player);
    }

    public void SpawnPlayersOnClient(Dictionary playerData) {

        int[] peers = Multiplayer.GetPeers();
        foreach (int peer in peers) {
            Player remotePlayer = Player.Create(_world, peer);
            AddChild(remotePlayer, true);
        }
        
        int peerId = Multiplayer.GetUniqueId();
        Player player = Player.Create(_world, peerId, playerData);
        AddChild(player, true);
        _localPlayer = player;
        LocalPlayerSpawned?.Invoke(player);
        RpcId(1, nameof(RpcHostHandleNewPeer));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostHandleNewPeer() {
        int senderId = Multiplayer.GetRemoteSenderId();
        
        Player remotePlayer = Player.Create(_world, senderId);
        AddChild(remotePlayer, true);
        _localPlayer.AddPeerToSynchronizer(senderId);
        PlayerSpawnedOnServer?.Invoke(remotePlayer);
        
        int[] peers = Multiplayer.GetPeers();
        foreach (int peerId in peers) {
            if (peerId == senderId) continue;
            RpcId(peerId, nameof(RpcSpawnNewPlayer), senderId);
        }
    }

    [Rpc]
    private void RpcSpawnNewPlayer(int peerId) {
        Player remotePlayer = Player.Create(_world, peerId);
        AddChild(remotePlayer, true);
        _localPlayer.AddPeerToSynchronizer(peerId);
    }
}