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
        if (!_world.IsHost) return;
        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost;
        };
    }
    
    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionState.Build.HostPlacePropAction 
            += OnHostPlaceProp;
        player.ActionState.Gather.HostGatherPropAction
            += OnHostGatherProp;
        player.TreeExiting += () => {
            player.ActionState.Build.HostPlacePropAction 
                -= OnHostPlaceProp;
            player.ActionState.Gather.HostGatherPropAction
                -= OnHostGatherProp;
        };
    }

    private void OnHostPlaceProp(Item item, Vector2I coords) {
        Prop newProp = Prop.Create(item, coords);
        foreach (Vector2I cell in newProp.Cells) {
            PropCells[cell] = newProp;
        }
        AddChild(newProp);
        Rpc(nameof(RpcClientsPlaceProp), item.ToDictionary(), coords);
    }

    [Rpc]
    private void RpcClientsPlaceProp(Dictionary itemDict, Vector2I coords) {
        Item item = Item.FromDictionary(itemDict);
        Prop newProp = Prop.Create(item, coords);
        foreach (Vector2I cell in newProp.Cells) {
            PropCells[cell] = newProp;
        }
        AddChild(newProp);
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