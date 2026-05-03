using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;
    private int _pickupCount;

    public event Action<Vector2I> ServerPickupCreated;
    public delegate void CellMovedDelegate(Vector2I newCoords, Vector2I oldCoords);
    public event CellMovedDelegate ServerPickupMoved;
    public event Action<Vector2I> ServerPickupDestroyed;

    public override void _Ready() {
        if (_world.IsHost) {
            _world.BlockManager.BlockDestroyed += HostOnBlockDestroyed;
            _world.PlayerManager.PlayerSpawnedOnHost += OnPlayedSpawnedOnHost;
            _world.PropManager.HostPropDestroyed += OnHostPropDestroyed;
            _world.BlockManager.WallDestroyed += HostOnBlockDestroyed;
            TreeExiting += () => {
                _world.BlockManager.BlockDestroyed -= HostOnBlockDestroyed;
                _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayedSpawnedOnHost;
                _world.PropManager.HostPropDestroyed -= OnHostPropDestroyed;
                _world.BlockManager.WallDestroyed -= HostOnBlockDestroyed;
            };

            ProcessMode = ProcessModeEnum.Always;
        } else {
            ProcessMode = ProcessModeEnum.Disabled;
            RpcId(1, nameof(RpcHostRequestWorldData));
        }
    }

    private void OnPlayedSpawnedOnHost(Player player) {
        player.ServerPickupArea.CollectedPickup += HostOnPlayerCollectedPickup;
        player.TreeExiting += () => { player.ServerPickupArea.CollectedPickup -= HostOnPlayerCollectedPickup; };
    }

    private void HostOnPlayerCollectedPickup(PickupEntity pickup) {
        _activePickups.Remove(pickup);
        pickup.QueueFreeAllPeers();
        ServerPickupDestroyed?.Invoke(pickup.Coords);
    }

    private void HostOnBlockDestroyed(Vector2I coords, ushort itemId) {
        Vector2 position = new(
            (coords.X + 0.5f) * Game.BlockSize,
            (coords.Y + 0.5f) * Game.BlockSize
        );
        _pickupCount++;
        Rpc(nameof(RpcAllCreatePickup),
            position, itemId, _pickupCount);
        ServerPickupCreated?.Invoke(coords);
    }

    private void OnHostPropDestroyed(Item item, Vector2I coords) {
        if (!item.GetProperty<ItemProp>().DoesDropSelf) return;
        Vector2 position = new(
            (coords.X + 0.5f) * Game.BlockSize,
            (coords.Y + 0.5f) * Game.BlockSize);
        _pickupCount++;
        ushort itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcAllCreatePickup),
            position, itemId, _pickupCount);
        ServerPickupCreated?.Invoke(coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreatePickup(
        Vector2 position, ushort itemId, int pickupCount
    ) {
        PickupEntity pickup =
            Data.PackedScenes.Pickup.Instantiate<PickupEntity>();
        Item item = _world.ItemIdBimap.GetItem(itemId);
        pickup.Position = position;
        pickup.Coords = new Vector2I(
            (int)(position.X / Game.BlockSize - 0.5f),
            (int)(position.Y / Game.BlockSize - 0.5f)
        );
        pickup.Item = item;
        pickup.Name = $"{pickupCount}";
        int[] peers = Multiplayer.GetPeers();
        foreach (int peerId in peers) {
            pickup.Synchronizer.SetVisibilityFor(peerId, true);
        }

        AddChild(pickup);
        _activePickups.Add(pickup);
    }

    public override void _PhysicsProcess(double delta) {
        foreach (PickupEntity pickup in _activePickups) {
            Vector2I newCoords = new(
                (int)(pickup.Position.X / Game.BlockSize - 0.5f),
                (int)(pickup.Position.Y / Game.BlockSize - 0.5f)
            );
            if (pickup.Coords == newCoords) continue;

            ServerPickupMoved?.Invoke(newCoords, pickup.Coords);
            pickup.Coords = newCoords;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostRequestWorldData() {
        int senderId = Multiplayer.GetRemoteSenderId();
        Array<Dictionary> pickupData = new();
        foreach (PickupEntity activePickup in _activePickups) {
            Dictionary dict = new();
            dict["ItemId"] = _world.ItemIdBimap.GetId(activePickup.Item);
            dict["Name"] = activePickup.Name;
            pickupData.Add(dict);
        }

        RpcId(senderId, nameof(RpcClientProcessPickupData), pickupData);
    }

    [Rpc]
    private void RpcClientProcessPickupData(Array<Dictionary> pickupArray) {
        foreach (Dictionary pickupDict in pickupArray) {
            int pickupId = pickupDict["Name"].ToString().ToInt();
            ushort itemId = (ushort)pickupDict["ItemId"];
            RpcAllCreatePickup(Vector2.Zero, itemId, pickupId);
        }

        RpcId(1, nameof(RpcHostAddClientToSync));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostAddClientToSync() {
        int senderId = Multiplayer.GetRemoteSenderId();
        // enable every pickup sync
        foreach (PickupEntity pickup in _activePickups) {
            pickup.Synchronizer.SetVisibilityFor(senderId, true);
        }
    }
}