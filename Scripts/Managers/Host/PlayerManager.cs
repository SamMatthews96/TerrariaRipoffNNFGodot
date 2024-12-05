using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node {
    [Export] private PackedScene _hostPlayerPackedScene;
    //@todo default spawn position should be taken from world
    public Vector2 DefaultSpawnPosition { get; private set; }

    public event Action<PlayerInfo> BeforePlayerSpawned;
    public event Action<Player> PlayerSpawned;

    public override void _Ready() {
        Manager.Instance.Game.PlayerConnected += OnGamePlayerConnected;
    }
    // @todo does this need refactor
    public void Initialize(Dictionary worldDictionary) {
        DefaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
    }

    private void OnGamePlayerConnected(PlayerInfo playerInfo, int peerId) {
        BeforePlayerSpawned?.Invoke(playerInfo);

        Vector2 spawnPosition = DefaultSpawnPosition * Game.BlockSize;
        Player player = Player.New(_hostPlayerPackedScene)
            .WithPeerId(peerId)
            .WithSpawnPosition(spawnPosition)
            .Build();

        PlayerSpawned?.Invoke(player);
    }
}