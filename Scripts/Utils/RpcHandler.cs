using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TerrariaRipoffNNF;

public record BufferedRpc {
    public Node Node;
    public StringName Method;
    public Variant[] Args;
}

public static class RpcHandler {
    private static Dictionary<int, List<BufferedRpc>> _bufferedRpcs;

    public static void Init() {
        _bufferedRpcs = new Dictionary<int, List<BufferedRpc>>();
    }

    public static void AddNewPeer(int peerId) {
        _bufferedRpcs[peerId] = new List<BufferedRpc>();
    }

    public static void RemoveNewPeer(int peerId) {
        _bufferedRpcs[peerId].ForEach(rpc => {
            rpc.Node.RpcId(peerId, rpc.Method, rpc.Args);
        });
        _bufferedRpcs.Remove(peerId);
    }

    public static void RpcAllPeers(
        this Node node,
        StringName method,
        params Variant[] args
    ) {
        if (!node.Multiplayer.IsServer()) {
            throw new Exception("RpcPeers can only be called on the server");
        }

        int[] allPeers = node.Multiplayer.GetPeers();
        IEnumerable<int> peers = allPeers.Append(1);

        foreach (int peer in peers) {
            if (_bufferedRpcs.ContainsKey(peer)) {
                BufferedRpc bufferedRpc = new() {
                    Node = node,
                    Method = method,
                    Args = args
                };
                _bufferedRpcs[peer].Add(bufferedRpc);;
            } else {
                node.RpcId(peer, method, args);
            }
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