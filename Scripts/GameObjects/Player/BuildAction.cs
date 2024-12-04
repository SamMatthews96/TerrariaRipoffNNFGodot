using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void GatherActionAttempt(Vector2 mouseWorldPosition) {
        
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        
    }
}