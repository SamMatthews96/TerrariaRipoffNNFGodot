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
            Array<Dictionary> breakables =
                _world.WorldData["breakables"].AsGodotArray<Dictionary>();
            foreach (Dictionary breakableDict in breakables) {
                int id = (int)breakableDict["id"];
                int x = (int)breakableDict["x"];
                int y = (int)breakableDict["y"];
                Breakable breakable = Data.Breakables.GetById(id);
                Vector2I coords = new(x, y);
                BreakableProp prop = BreakableProp.Create(breakable, coords);
                AddProp(prop, coords);
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

        Dictionary packet = new();
        Array<Dictionary> breakableData = new();
        Array<Dictionary> propData = new();
        foreach ((Vector2I cell, Prop prop) in _props) {
            int id;
            Dictionary propDict;
            switch (prop) {
                case BreakableProp breakable:
                    id = breakable.Breakable.Id;
                    propDict = new() {
                        { "coords", cell },
                        { "id", id }
                    };
                    breakableData.Add(propDict);
                    break;
                case PlaceableProp:
                    id = _world.ItemIdBimap.GetId(prop.Item);
                    propDict = new() {
                        { "coords", cell },
                        { "id", id }
                    };
                    propData.Add(propDict);
                    break;
                default:
                    throw new Exception("invalid prop type");
            }
        }

        packet["breakables"] = breakableData;
        packet["props"] = propData;

        RpcId(senderId, nameof(RpcClientProcessPropData), packet);
        
    }

    [Rpc]
    private void RpcClientProcessPropData(Dictionary packet) {
        Array<Dictionary> breakables = 
            packet["breakables"].AsGodotArray<Dictionary>();
        foreach (Dictionary breakableDict in breakables) {
            int id = (int)breakableDict["id"];
            Vector2I coords = (Vector2I)breakableDict["coords"];
            Breakable breakable = Data.Breakables.GetById(id);
            BreakableProp prop = BreakableProp.Create(breakable, coords);
            AddProp(prop, coords);
        }
        
        Array<Dictionary> props = 
            packet["props"].AsGodotArray<Dictionary>();
        foreach (Dictionary propDict in props) {
            ushort itemId = (ushort)propDict["id"];
            Vector2I coords = (Vector2I)propDict["coords"];
            Item item = _world.ItemIdBimap.GetItem(itemId);
            PlaceableProp prop = PlaceableProp.Create(item, coords);
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
        PlaceableProp newPlaceableProp = PlaceableProp.Create(item, coords);
        AddProp(newPlaceableProp, coords);
        ushort itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcClientsPlaceProp), itemId, coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(ushort itemId, Vector2I coords) {
        Item item = _world.ItemIdBimap.GetItem(itemId);
        PlaceableProp newPlaceableProp = PlaceableProp.Create(item, coords);
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