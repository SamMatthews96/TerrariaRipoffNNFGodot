using Godot;
using System;
using Newtonsoft.Json;
using TerrariaRipoffNNF;
using JsonSerializer = System.Text.Json.JsonSerializer;

public partial class Test : Node
{
    public override void _Ready() {
        // test jsonserialiser
        IntVector resource = new IntVector(4,6);
        
        string json = JsonSerializer.Serialize(resource);
        GD.Print(json);
        // test deserialiser
        // test nested properties
        
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void TestVariantPassing(IntVector resource) {
        GD.Print(resource.X, resource.Y);
    }
}
