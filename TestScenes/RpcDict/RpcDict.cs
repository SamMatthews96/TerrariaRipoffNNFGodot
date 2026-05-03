using Godot;

namespace TerrariaRipoffNNF.RpcDict;

public partial class RpcDict : Node {
    public override void _Ready() {
        Item item = Data.Items.Earth;
        
        Rpc(nameof(RpcTest), item);
    }

    [Rpc(CallLocal = true)]
    private void RpcTest(Item item) {
        GD.Print("RPC Test", item);
        GD.Print(item.ResourcePath);
        GD.Print(item.Name);
        GD.Print(item.InventorySpace);
    }
}