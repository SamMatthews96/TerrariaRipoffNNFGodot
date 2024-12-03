using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    public event Action<IntVector, float> GatherAttempted;

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        // @todo make the gather action periodic while the mouse is held down
        RpcId(Manager.HostId, nameof(GatherActionAttempt), mouseWorldPosition);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GatherActionAttempt(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;
        
        // get properties of player that aren't defined yet
        // mine speed, range, damage
        // for now, use constants
        float mineSpeed = 1;
        float range = 1;
        float damage = 100;
        
        GatherAttempted?.Invoke(coords, damage);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        GD.Print("end");

    }
}