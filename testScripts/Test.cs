using Godot;
using System;
using TerrariaRipoffNNF;

public partial class Test : Node
{
    public override void _Ready() {
        IntVector test = new (4,6);
        test.ResourceName = "TestResource";
        GD.Print(test.ResourceName);
        RpcId(Multiplayer.GetUniqueId(),nameof(TestVariantPassing), test);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void TestVariantPassing(IntVector resource) {
        GD.Print(resource.X, resource.Y);
    }
}
