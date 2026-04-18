using Godot;
using Godot.Collections;

public partial class RpcDataTest : Node {
    // using this to test what kinds of data can be sent over RPC
    
    public override void _Ready() {
        Vector2I myData = new(1, 2);
        Rpc(nameof(MyRpc), myData);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void MyRpc(Vector2I data) {
        GD.Print(data);
    }
}