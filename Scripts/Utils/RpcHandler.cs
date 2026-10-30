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
    private static MultiplayerApi _multiplayer;
    private static Dictionary<int, List<MultiplayerSynchronizer>>
        _bufferedSynchronizers;

    public static void Init(Game game) {
        _multiplayer = game.Multiplayer;
        _bufferedRpcs =
            new Dictionary<int, List<BufferedRpc>>();
        _bufferedSynchronizers =
            new Dictionary<int, List<MultiplayerSynchronizer>>();
    }

    public static void AddNewPeer(int peerId) {
        _bufferedRpcs[peerId] = new List<BufferedRpc>();
        _bufferedSynchronizers[peerId] = new List<MultiplayerSynchronizer>();
    }

    public static void RemoveNewPeer(int peerId) {
        _bufferedRpcs[peerId].ForEach(rpc => { rpc.Node.RpcId(peerId, rpc.Method, rpc.Args); });
        _bufferedRpcs.Remove(peerId);
        _bufferedSynchronizers[peerId].ForEach(synchronizer => { synchronizer.SetVisibilityFor(peerId, false); });
        _bufferedSynchronizers.Remove(peerId);
    }

    public static void RpcAll(
        this Node node,
        StringName method,
        params Variant[] args
    ) {
        if (!_multiplayer.IsServer()) {
            throw new Exception("RpcPeers can only be called on the server");
        }

        int[] allPeers = _multiplayer.GetPeers();
        IEnumerable<int> peers = allPeers.Append(1);

        foreach (int peer in peers) {
            if (_bufferedRpcs.ContainsKey(peer)) {
                BufferedRpc bufferedRpc = new() {
                    Node = node,
                    Method = method,
                    Args = args
                };
                _bufferedRpcs[peer].Add(bufferedRpc);
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

    public static void SetVisible(this MultiplayerSynchronizer synchronizer) {
        int[] peers = _multiplayer.GetPeers();
        foreach (int peerId in peers) {
            if (_bufferedSynchronizers.ContainsKey(peerId)) {
                _bufferedSynchronizers[peerId].Add(synchronizer);
            } else {
                synchronizer.SetVisibilityFor(peerId, true);
            }
        }

        synchronizer.SetVisibilityFor(Game.HostId, true);
    }
}