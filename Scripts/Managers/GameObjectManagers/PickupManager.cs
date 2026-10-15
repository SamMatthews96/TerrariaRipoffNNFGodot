using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;
    private BlockManager _blockManager;
    private PlayerManager _playerManager;
    private PropManager _propManager;
    private ItemIdBimap _itemIdBimap;
    private int _pickupCount;

    public event Action<Vector2I> ServerPickupCreated;
    public delegate void CellMovedDelegate(Vector2I newCoords, Vector2I oldCoords);
    public event CellMovedDelegate ServerPickupMoved;
    public event Action<Vector2I> ServerPickupDestroyed;

    public override void _Ready() {
        _blockManager = _world.BlockManager;
        _playerManager = _world.PlayerManager;
        _propManager = _world.PropManager;
        _itemIdBimap = _world.ItemIdBimap;

        if (_world.IsHost) {
            _blockManager.BlockDestroyed += HostOnBlockDestroyed;
            _playerManager.PlayerSpawnedOnHost += OnPlayedSpawnedOnHost;
            _propManager.HostPropDestroyed += OnHostPropDestroyed;
            _blockManager.WallDestroyed += HostOnBlockDestroyed;
            TreeExiting += () => {
                _blockManager.BlockDestroyed -= HostOnBlockDestroyed;
                _playerManager.PlayerSpawnedOnHost -= OnPlayedSpawnedOnHost;
                _propManager.HostPropDestroyed -= OnHostPropDestroyed;
                _blockManager.WallDestroyed -= HostOnBlockDestroyed;
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
        HostCreatePickup(coords, itemId);
    }

    private void OnHostPropDestroyed(ActiveProp activeProp, Vector2I coords) {
        ItemProp itemProp = activeProp.Item.GetProperty<ItemProp>();
        if (itemProp.TryGetProperty(out PropDrop propDrop)) {
            ushort itemId = _itemIdBimap.GetId(propDrop.Item);
            HostCreatePickup(coords, itemId);
        }
        if (itemProp.DoesDropSelf) {
            ushort itemId = _itemIdBimap.GetId(activeProp.Item);
            HostCreatePickup(coords, itemId);
        }
    }

    private void HostCreatePickup(Vector2I coords, ushort itemId) {
        Vector2 position = new(
            (coords.X + 0.5f) * Game.BlockSize,
            (coords.Y + 0.5f) * Game.BlockSize);
        _pickupCount++;
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
        Item item = _itemIdBimap.GetItem(itemId);
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
            dict["ItemId"] = _itemIdBimap.GetId(activePickup.Item);
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