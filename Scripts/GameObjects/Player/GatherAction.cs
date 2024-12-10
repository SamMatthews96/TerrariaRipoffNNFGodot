using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<IntVector, float> GatherAttempted;

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        if (Player.CurrentEquipment.Pickaxe is null) return;
        
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;

        MiningSlot miningSlot = Player.CurrentEquipment.Pickaxe.GetProperty<MiningSlot>();
       
        if (miningSlot.Range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(Manager.HostId, nameof(HostGatherAttempted),
                coords.ToSerialised(), miningSlot.MiningPower);
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