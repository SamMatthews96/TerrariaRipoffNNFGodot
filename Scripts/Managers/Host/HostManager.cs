using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Utils;
using PlayerInfo = TerrariaRipoffNNF.Scripts.Resources.PlayerInfo;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;



public partial class HostManager : Node {
    public static HostManager Instance { get; private set; }

    public Vector2 DefaultSpawnPosition { get; private set; }

    public static void RequireHost() {
        if (!GameManager.Instance.IsHost) {
            throw new Exception("[20240813.1408.1] Method should only be called on the host");
        }
    }

    public override void _EnterTree() {
        RequireHost();
        if (Instance is not null) {
            throw new Exception("[20240808.1730.1] GameManager already instantiated");
        }

        Instance = this;
    }

    public void Initialize(Dictionary worldDictionary) {
        DefaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());

        HostBlockManager.Instance.Initialize(worldDictionary);
        HostPickupManager.Instance.Initialize();

        GameManager.Instance.PlayerJoined += OnGameManagerPlayerJoined;
    }

    private void OnGameManagerPlayerJoined(PlayerInfo playerInfo, int peerId) {
        IntVector spawnPosition = new(DefaultSpawnPosition);
        List<IntVector> region = GameManager.Instance.Region.GetRegion(
            spawnPosition, HostBlockManager.BlockSpawnDistance);
        HostBlockManager.Instance.SpawnBlocksInRegion(region);
        HostPlayerManager.Instance.SpawnPlayer(peerId, playerInfo);
    }
}