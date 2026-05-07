using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

using PropsByCoords = Dictionary<Vector2I, ActiveProp>;
using ColumnByX = Dictionary<int, Array<int>>;
using ColumnByXByBlockId =
    Dictionary<ushort, Dictionary<int, Array<int>>>;

public partial class PropManager : Node {
    [Export] private World _world;
    public PropsByCoords PropCells { get; private set; } = new();
    public PropsByCoords Props { get; private set; } = new();

    public event Action<ActiveProp, Vector2I> HostPropDestroyed;

    public override void _Ready() {
        if (_world.IsHost) {
            ColumnByXByBlockId props =
                (ColumnByXByBlockId)_world.WorldData["props"];
            foreach ((ushort itemId, ColumnByX xDict) in props) {
                Item item = _world.ItemIdBimap.GetItem(itemId);
                foreach ((int x, Array<int> yArray) in xDict) {
                    foreach (int y in yArray) {
                        Vector2I coords = new(x, y);
                        AddProp(item, coords);
                    }
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
        foreach ((Vector2I cell, ActiveProp prop) in Props) {
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
            AddProp(item, coords);
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
        AddProp(item, coords);
        ushort itemId = _world.ItemIdBimap.GetId(item);
        Rpc(nameof(RpcClientsPlaceProp), itemId, coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(ushort itemId, Vector2I coords) {
        Item item = _world.ItemIdBimap.GetItem(itemId);
        AddProp(item, coords);
    }

    private void OnHostGatherProp(Vector2I coords, float damage) {
        ActiveProp activeProp = PropCells[coords];
        Rpc(nameof(RpcClientsGatherProp), coords);
        HostPropDestroyed?.Invoke(activeProp, activeProp.Anchor);
    }

    [Rpc(CallLocal = true)]
    private void RpcClientsGatherProp(Vector2I coords) {
        RemoveProp(coords);
    }

    private ActiveProp AddProp(Item item, Vector2I coords) {
        ActiveProp activeProp = ActiveProp.Create(item, coords);

        foreach (Vector2I cell in activeProp.Cells) {
            PropCells[cell] = activeProp;
        }

        Props[coords] = activeProp;
        ItemProp itemProp = item.GetProperty<ItemProp>();
        foreach (PropProperty propProperty in itemProp.Properties) {
            propProperty.Apply(activeProp, _world);
        }

        AddChild(activeProp);
        return activeProp;
    }

    private void RemoveProp(Vector2I coords) {
        ActiveProp activeProp = PropCells[coords];
        foreach (Vector2I cell in activeProp.Cells) {
            PropCells.Remove(cell);
        }

        Props.Remove(activeProp.Anchor);
        activeProp.QueueFree();
    }
}