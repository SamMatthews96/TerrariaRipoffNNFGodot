using Godot;

namespace TerrariaRipoffNNF.TestScenes.RpcArgs;

// Sending resources across RPC will not work
public partial class SendRpc : Node {
    [Export] private NetworkTest _networkTest;
    // [Export] private Recipe _recipe;

    public override void _Ready() {
        _networkTest.ClientStarted += OnJoined;
    }

    private void OnJoined() {
        // Rpc(nameof(RpcTest), _recipe);
    }

    // [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    // private void RpcTest(Recipe recipe) {
    //     GD.Print(recipe);
    // }
}