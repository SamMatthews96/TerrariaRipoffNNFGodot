using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class PropManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, Prop> PropCells { get; private set; }

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
                foreach (Vector2I propCell in prop.Cells) {
                    PropCells[propCell] = prop;
                }

                AddChild(prop);
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
        foreach ((Vector2I cell, Prop prop) in PropCells) {
            switch (prop) {
                case BreakableProp breakable:
                    int id = breakable.Breakable.Id;
                    Dictionary propDict = new() {
                        { "coords", cell },
                        { "id", id }
                    };
                    breakableData.Add(propDict);
                    break;
                case PlaceableProp:
                    // int id = _world.ItemIdBimap.GetId(prop.Item);
                    break;
                default:
                    throw new Exception("invalid prop type");
            }
        }

        packet["breakables"] = breakableData;

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
            foreach (Vector2I propCell in prop.Cells) {
                PropCells[propCell] = prop;
            }
            AddChild(prop);
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
        foreach (Vector2I cell in newPlaceableProp.Cells) {
            PropCells[cell] = newPlaceableProp;
        }

        AddChild(newPlaceableProp);
        ushort itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcClientsPlaceProp), itemId, coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(ushort itemId, Vector2I coords) {
        Item item = _world.ItemIdBimap.GetItem(itemId);
        PlaceableProp newPlaceableProp = PlaceableProp.Create(item, coords);
        foreach (Vector2I cell in newPlaceableProp.Cells) {
            PropCells[cell] = newPlaceableProp;
        }

        AddChild(newPlaceableProp);
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
}