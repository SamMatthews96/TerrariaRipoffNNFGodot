using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<IntVector, float> GatherAttempted;

    private Game _game;
    
    public void InitAsLocal(Game game) {
        _game = game;
    }
    
    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        float miningPowerTemp = 10f;
        float miningRangeTemp = 8f;
        if (miningRangeTemp >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(SceneManager.HostId, nameof(HostGatherAttempted),
                coords.ToSerialised(), miningPowerTemp);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void HostGatherAttempted(Array intVectorArray, float damage) {
        IntVector coords = new(intVectorArray);

        GatherAttempted?.Invoke(coords, damage);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
    }
}