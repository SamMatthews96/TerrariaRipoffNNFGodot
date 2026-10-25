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
    private PlayerManager _playerManager;
    private ItemIdBimap _itemIdBimap;

    private PropsByCoords _propCells;
    private PropsByCoords _props;

    public event Action<ActiveProp, Vector2I> HostPropDestroyed;
    
    public bool IsPropAt(Vector2I coords) => _propCells.ContainsKey(coords);
    
    public Dictionary<string, Dictionary> SerializeProps() {
        Dictionary<string, Dictionary> groupedByItemId = new();

        foreach ((Vector2I coords, ActiveProp prop) in _props) {
            Item item = prop.Item;
            string itemId = _itemIdBimap.GetId(item).ToString();

            if (!groupedByItemId.ContainsKey(itemId)) {
                groupedByItemId[itemId] = new Dictionary();
            }

            if (!groupedByItemId[itemId].ContainsKey($"{coords.X}")) {
                groupedByItemId[itemId][$"{coords.X}"] = new Array();
            }

            ((Array)groupedByItemId[itemId][$"{coords.X}"]).Add(coords.Y);
        }

        return groupedByItemId;
    }

    public override void _Ready() {
        _playerManager = _world.PlayerManager;
        _itemIdBimap = _world.ItemIdBimap;
        _propCells = new PropsByCoords();
        _props = new PropsByCoords();
        
        if (_world.IsHost) {
            ColumnByXByBlockId props =
                (ColumnByXByBlockId)_world.WorldData["props"];
            foreach ((ushort itemId, ColumnByX xDict) in props) {
                Item item = _itemIdBimap.GetItem(itemId);
                foreach ((int x, Array<int> yArray) in xDict) {
                    foreach (int y in yArray) {
                        Vector2I coords = new(x, y);
                        AddProp(item, coords);
                    }
                }
            }

            _playerManager.HostPlayerSpawned += OnPlayerSpawnedOnHost;
            TreeExiting += () => { _playerManager.HostPlayerSpawned -= OnPlayerSpawnedOnHost; };
        } else {
            RpcId(1, nameof(RpcHostRequestWorldData));
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcHostRequestWorldData() {
        int senderId = Multiplayer.GetRemoteSenderId();

        Array<Dictionary> propData = new();
        foreach ((Vector2I cell, ActiveProp prop) in _props) {
            int id = _itemIdBimap.GetId(prop.Item);
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
            Item item = _itemIdBimap.GetItem(itemId);
            AddProp(item, coords);
        }
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.PropPlaced += OnHostPlaceProp;
        player.PropGathered += OnHostGatherProp;
        player.TreeExiting += () => {
            player.PropPlaced -= OnHostPlaceProp;
            player.PropGathered -= OnHostGatherProp;
        };
    }

    private void OnHostPlaceProp(Item item, Vector2I coords) {
        AddProp(item, coords);
        ushort itemId = _itemIdBimap.GetId(item);
        Rpc(nameof(RpcClientsPlaceProp), itemId, coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(ushort itemId, Vector2I coords) {
        Item item = _itemIdBimap.GetItem(itemId);
        AddProp(item, coords);
    }

    private void OnHostGatherProp(Vector2I coords, float damage) {
        ActiveProp activeProp = _propCells[coords];
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
            _propCells[cell] = activeProp;
        }

        _props[coords] = activeProp;
        ItemProp itemProp = item.GetProperty<ItemProp>();
        foreach (PropProperty propProperty in itemProp.Properties) {
            propProperty.Apply(activeProp, _world);
        }

        AddChild(activeProp);
        return activeProp;
    }

    private void RemoveProp(Vector2I coords) {
        ActiveProp activeProp = _propCells[coords];
        foreach (Vector2I cell in activeProp.Cells) {
            _propCells.Remove(cell);
        }

        _props.Remove(activeProp.Anchor);
        activeProp.QueueFree();
    }
}