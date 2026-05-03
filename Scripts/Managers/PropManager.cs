using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class PropManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, Prop> PropCells { get; private set; }
    private Dictionary<Vector2I, Prop> _props = new();

    public event Action<Item, Vector2I> HostPropDestroyed;

    public override void _Ready() {
        PropCells = new Dictionary<Vector2I, Prop>();
        if (_world.IsHost) {
            Dictionary<ushort, Array> props =
                (Dictionary<ushort, Array>)_world.WorldData["props"];
            foreach ((ushort itemId, Array cellArray) in props) {
                foreach (Array cell in cellArray) {
                    int x = (int)cell[0];
                    int y = (int)cell[1];
                    Vector2I coords = new(x, y);
                    Item item = _world.ItemIdBimap.GetItem(itemId);
                    Prop prop = Prop.Create(item, coords);
                    AddProp(prop, coords);
                }
            }

            _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
            TreeExiting += () => { _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost; };
        } else {
            RpcId(1, nameof(RpcHostRequestWorldData));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostRequestWorldData() {
        int senderId = Multiplayer.GetRemoteSenderId();

        Array<Dictionary> propData = new();
        foreach ((Vector2I cell, Prop prop) in _props) {
            int id = _world.ItemIdBimap.GetId(prop.Item);
            Dictionary propDict = new() {
                { "coords", cell },
                { "id", id }
            };
            propData.Add(propDict);
        }

        RpcId(senderId, nameof(RpcClientProcessPropData), 
            propData);
    }

    [Rpc]
    private void RpcClientProcessPropData(Array<Dictionary> packet) {
        foreach (Dictionary propDict in packet) {
            ushort itemId = (ushort)propDict["id"];
            Vector2I coords = (Vector2I)propDict["coords"];
            Item item = _world.ItemIdBimap.GetItem(itemId);
            Prop prop = Prop.Create(item, coords);
            AddProp(prop, coords);
        }
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionState.Build.HostPlaceProp += OnHostPlaceProp;
        player.ActionState.Gather.HostGatherProp += OnHostGatherProp;
        player.TreeExiting += () => {
            player.ActionState.Build.HostPlaceProp -= OnHostPlaceProp;
            player.ActionState.Gather.HostGatherProp -= OnHostGatherProp;
        };
    }

    private void OnHostPlaceProp(Item item, Vector2I coords) {
        Prop newPlaceableProp = Prop.Create(item, coords);
        AddProp(newPlaceableProp, coords);
        ushort itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcClientsPlaceProp), itemId, coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(ushort itemId, Vector2I coords) {
        Item item = _world.ItemIdBimap.GetItem(itemId);
        Prop newPlaceableProp = Prop.Create(item, coords);
        AddProp(newPlaceableProp, coords);
    }

    private void OnHostGatherProp(Vector2I coords, float damage) {
        Prop prop = PropCells[coords];
        Rpc(nameof(RpcClientsGatherProp), coords);
        HostPropDestroyed?.Invoke(prop.Item, coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcClientsGatherProp(Vector2I coords) {
        Prop prop = PropCells[coords];
        foreach (Vector2I cell in prop.Cells) {
            PropCells.Remove(cell);
        }

        prop.QueueFree();
    }

    private void AddProp(Prop prop, Vector2I coords) {
        foreach (Vector2I cell in prop.Cells) {
            PropCells[cell] = prop;
        }

        _props[coords] = prop;
        AddChild(prop);
    }
}