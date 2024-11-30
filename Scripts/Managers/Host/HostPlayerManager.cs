using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using PlayerInfo = TerrariaRipoffNNF.Scripts.Resources.PlayerInfo;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostPlayerManager : Node {
    [Export] private PackedScene _hostPlayerPackedScene;

    public event Action<Player> PlayerSpawned;

    public void SpawnPlayer(int peerId, PlayerInfo playerInfo) {
        Vector2 spawnPosition = Host.Instance.DefaultSpawnPosition * Game.BlockSize;
        Player player = Player.New(Manager.Instance.Game.PlayerParent, _hostPlayerPackedScene)
            .WithPeerId(peerId)
            .WithSpawnPosition(spawnPosition)
            .Build();
        
        PlayerSpawned?.Invoke(player);
    }
}