using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PropManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, Prop> PropCells { get; private set; }

    public event Action<Item, Vector2I> HostPropDestroyed;

    public override void _Ready() {
        PropCells = new Dictionary<Vector2I, Prop>();
        
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
        
        if (!_world.IsHost) return;
        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost;
        };
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
        Rpc(nameof(RpcClientsPlaceProp), item.ToDictionary(), coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(Dictionary itemDict, Vector2I coords) {
        Item item = Item.FromDictionary(itemDict);
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