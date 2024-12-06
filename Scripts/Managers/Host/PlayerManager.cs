using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node {
    [Export] private PackedScene _hostPlayerPackedScene;
    public Vector2 DefaultSpawnPosition { get; private set; }

    public event Action<Dictionary> BeforePlayerSpawned;
    public event Action<Player> PlayerSpawned;

    public override void _Ready() {
        Manager.Instance.Game.PlayerConnected += OnGamePlayerConnected;
    }
    
    public void Initialize(Dictionary worldDictionary) {
        DefaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
    }

    private void OnGamePlayerConnected(Dictionary playerInfo, int peerId) {
        BeforePlayerSpawned?.Invoke(playerInfo);

        Vector2 spawnPosition = DefaultSpawnPosition * Game.BlockSize;
        Player player = Player.New(_hostPlayerPackedScene)
            .WithPeerId(peerId)
            .WithSpawnPosition(spawnPosition)
            .Build();

        PlayerSpawned?.Invoke(player);
    }
}