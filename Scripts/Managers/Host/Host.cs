using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Host : Node {
    
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }

    public Vector2 DefaultSpawnPosition { get; private set; }

    public static void RequireHost() {
        if (!Manager.Instance.Game.IsHost) {
            throw new Exception("[20240813.1408.1] Method should only be called on the host");
        }
    }

    public override void _EnterTree() {
        RequireHost();
    }

    public void Initialize(Dictionary worldDictionary) {
        DefaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());

        BlockManager.Initialize(worldDictionary);
        PickupManager.Initialize();

        Manager.Instance.Game.PlayerJoined += OnGameManagerPlayerJoined;
    }

    private void OnGameManagerPlayerJoined(PlayerInfo playerInfo, int peerId) {
        IntVector spawnPosition = new(DefaultSpawnPosition);
        List<IntVector> region = Manager.Instance.Game.Region.GetRegion(
            spawnPosition, BlockManager.BlockSpawnDistance);
        BlockManager.SpawnBlocksInRegion(region);
        PlayerManager.SpawnPlayer(peerId, playerInfo);
    }
}