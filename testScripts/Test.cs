using Godot;
using TerrariaRipoffNNF;

public partial class Test : Node
{
    public override void _Ready() {
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void TestVariantPassing(IntVector resource) {
        GD.Print(resource.X, resource.Y);
    }
}
