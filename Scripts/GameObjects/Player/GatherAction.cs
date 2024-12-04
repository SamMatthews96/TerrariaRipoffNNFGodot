using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<IntVector, float> GatherAttempted;

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;
        
        // get properties of player that aren't defined yet
        // mine speed, range, damage
        // for now, use constants
        float mineSpeed = 1;
        float range = 1;
        float damage = 100;
        
        RpcId(Manager.HostId, nameof(HostGatherAttempted), 
            coords.ToSerialised(), damage);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void HostGatherAttempted(Array intVectorArray, float damage) {
        IntVector coords = new (intVectorArray);
        
        GatherAttempted?.Invoke(coords, damage);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        GD.Print("end");

    }
}