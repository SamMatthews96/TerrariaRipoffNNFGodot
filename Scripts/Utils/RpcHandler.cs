using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public static class RpcHandler {
    public static void RpcPeers(
        this Node node,
        IEnumerable<int> peers,
        StringName method,
        params Variant[] args
    ) {
        // int[] allPeers = node.Multiplayer.GetPeers();
        foreach (int peer in peers) {
            node.RpcId(peer, method, args);
        }
    }

    public static void RpcHost(
        this Node node,
        StringName method,
        params Variant[] args
    ) {
        node.RpcId(Game.HostId, method, args);
    }
}