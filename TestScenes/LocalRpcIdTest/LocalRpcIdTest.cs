using Godot;

namespace TerrariaRipoffNNF.LocalRpcIdTest;

// it will throw an error to rpc id yourself without CallLocal

public partial class LocalRpcIdTest : Node {
    public override void _Ready() {

        RpcId(1, nameof(RpcTest));
    }
    
    [Rpc]
    private void RpcTest() {
        GD.Print("RPC Test");
    }
}