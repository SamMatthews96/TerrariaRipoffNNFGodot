using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public delegate void CellMovedDelegate(Vector2I newCoords, Vector2I oldCoords);

public partial class PickupManager : Node2D {
    private readonly List<PickupEntity> _activePickups = new();
    [Export] private World _world;
    private BlockManager _blockManager;
    private PlayerManager _playerManager;
    private PropManager _propManager;
    private ItemIdBimap _itemIdBimap;
    private int _pickupCount;

    public event Action<Vector2I> ServerPickupCreated;
    public event CellMovedDelegate ServerPickupMoved;
    public event Action<Vector2I> ServerPickupDestroyed;

    public Array<Dictionary> Serialize() {
        Array<Dictionary> pickupData = new();
        foreach (PickupEntity activePickup in _activePickups) {
            Dictionary dict = new();
            dict["ItemId"] = _itemIdBimap.GetId(activePickup.Item);
            dict["Name"] = activePickup.Name.ToString().ToInt();
            dict["Position"] = new Array {
                activePickup.Position.X,
                activePickup.Position.Y
            };
            pickupData.Add(dict);
        }

        return pickupData;
    }

    public override void _Ready() {
        _blockManager = _world.BlockManager;
        _playerManager = _world.PlayerManager;
        _propManager = _world.PropManager;
        _itemIdBimap = _world.ItemIdBimap;

        Array<Dictionary> savedPickups =
            (Array<Dictionary>)_world.WorldData["pickups"];

        if (_world.IsHost) {
            _blockManager.BlockDestroyed += HostOnBlockDestroyed;
            _playerManager.HostPlayerSpawned += OnPlayedSpawnedOnHost;
            _propManager.HostPropDestroyed += OnHostPropDestroyed;
            _blockManager.WallDestroyed += HostOnBlockDestroyed;
            _world.AfterPeerConnected += OnAfterPeerConnected;
            TreeExiting += () => {
                _world.AfterPeerConnected -= OnAfterPeerConnected;
                _blockManager.BlockDestroyed -= HostOnBlockDestroyed;
                _playerManager.HostPlayerSpawned -= OnPlayedSpawnedOnHost;
                _propManager.HostPropDestroyed -= OnHostPropDestroyed;
                _blockManager.WallDestroyed -= HostOnBlockDestroyed;
            };

            foreach (Dictionary pickupDict in savedPickups) {
                Array<float> positionArray = pickupDict["Position"].AsGodotArray<float>();
                Vector2 position = new(positionArray[0], positionArray[1]);
                ushort itemId = (ushort)pickupDict["ItemId"];
                HostCreatePickup(position, itemId);
            }

            ProcessMode = ProcessModeEnum.Always;
        } else {
            foreach (Dictionary pickupDict in savedPickups) {
                Array<float> positionArray = pickupDict["Position"].AsGodotArray<float>();
                Vector2 position = new(positionArray[0], positionArray[1]);
                ushort itemId = (ushort)pickupDict["ItemId"];
                int pickupId = pickupDict["Name"].ToString().ToInt();
                CreatePickup(position, itemId, pickupId);
            }

            ProcessMode = ProcessModeEnum.Disabled;
        }
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

    private void OnPlayedSpawnedOnHost(Player player) {
        player.CollectedPickup += HostOnPlayerCollectedPickup;
        player.TreeExiting += () => { player.CollectedPickup -= HostOnPlayerCollectedPickup; };
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

    private void HostCreatePickup(Vector2 position, ushort itemId) {
        Vector2I coords = new(
            (int)(position.X / Game.BlockSize - 0.5f),
            (int)(position.Y / Game.BlockSize - 0.5f)
        );
        _pickupCount++;
        this.RpcAllPeers(nameof(CreatePickup),
            position, itemId, _pickupCount);
        ServerPickupCreated?.Invoke(coords);
    }

    private void HostCreatePickup(Vector2I coords, ushort itemId) {
        Vector2 position = new(
            (coords.X + 0.5f) * Game.BlockSize,
            (coords.Y + 0.5f) * Game.BlockSize);
        _pickupCount++;
        this.RpcAllPeers(nameof(CreatePickup),
            position, itemId, _pickupCount);
        ServerPickupCreated?.Invoke(coords);
    }

    [Rpc(CallLocal = true)]
    private void CreatePickup(
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

        if (_world.IsHost) {
            pickup.Synchronizer.SetVisible();
        } else {
            pickup.Synchronizer.SetVisibilityFor(1, true);
        }

        AddChild(pickup);
        _activePickups.Add(pickup);
    }
    
    private void OnAfterPeerConnected(int senderId) {
        foreach (PickupEntity pickup in _activePickups) {
            pickup.Synchronizer.SetVisibilityFor(senderId, true);
        }
    }
}