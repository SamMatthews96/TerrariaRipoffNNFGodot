using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using PlayerInfo = TerrariaRipoffNNF.Scripts.Resources.PlayerInfo;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostPlayerManager : Node {
    public static HostPlayerManager Instance { get; private set; }

    [Export] private PackedScene _hostPlayerPackedScene;

    public event Action<Player> PlayerSpawned;

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240814.0045.1] HostManager already instantiated");
        }
        Instance = this;
    }

    public void SpawnPlayer(int peerId, PlayerInfo playerInfo) {
        Vector2 spawnPosition = HostManager.Instance.DefaultSpawnPosition * GameManager.BlockSize;
        Player player = Player.New(GameManager.Instance.PlayerParent, _hostPlayerPackedScene)
            .WithPeerId(peerId)
            .WithSpawnPosition(spawnPosition)
            .Build();
        
        PlayerSpawned?.Invoke(player);
    }
}