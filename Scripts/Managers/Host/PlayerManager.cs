using System;
using Godot;
namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node {
    [Export] private PackedScene _hostPlayerPackedScene;

    public event Action<Player> PlayerSpawned;

    public void SpawnPlayer(int peerId, PlayerInfo playerInfo) {
        Vector2 spawnPosition = Manager.Instance.Game.Host.DefaultSpawnPosition * Game.BlockSize;
        Player player = Player.New(Manager.Instance.Game.PlayerParent, _hostPlayerPackedScene)
            .WithPeerId(peerId)
            .WithSpawnPosition(spawnPosition)
            .Build();
        
        PlayerSpawned?.Invoke(player);
    }
}