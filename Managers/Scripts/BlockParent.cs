using System;
using Godot;
using TerrariaRipoffNNF.GameObjects.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class BlockParent : Node {
    [Export] private MultiplayerSpawner _multiplayerSpawner;

    public override void _Ready() {
        _multiplayerSpawner.Spawned += OnMultiPlayerSpawnerSpawned;
    }

    private void OnMultiPlayerSpawnerSpawned(Node node) {
        if (node is not ActiveBlock activeBlock) {
            throw new Exception("[20240812.2105.1] multiplayer spawned node should be Activeblock");
        }

        RpcId(Manager.MultiplayerHostId, nameof(ServerHandleBlockSpawn),
            activeBlock.XCoordinate, activeBlock.YCoordinate);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServerHandleBlockSpawn(int xPosition, int yPosition) { }
}