using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PropManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, Prop> PropCells { get; private set; }

    public override void _Ready() {
        PropCells = new Dictionary<Vector2I, Prop>();
        if (!_world.IsHost) return;
        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost;
        };
    }
    
    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionController.BuildAction.HostPlacePropAction 
            += OnHostPlacePropAction;
        player.ActionController.GatherAction.HostGatherPropAction
            += OnHostGatherPropAction;
        player.TreeExiting += () => {
            player.ActionController.BuildAction.HostPlacePropAction 
                -= OnHostPlacePropAction;
            player.ActionController.GatherAction.HostGatherPropAction
                -= OnHostGatherPropAction;
        };
    }

    private void OnHostPlacePropAction(Item item, Vector2I coords) {
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
    
    private void OnHostGatherPropAction(Vector2I coords, float damage) {
        Rpc(nameof(RpcClientsGatherProp), coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcClientsGatherProp(Vector2I coords) {
        Prop prop = PropCells[coords];
        foreach (Vector2I cell in prop.Cells) {
            PropCells[cell] = null;
        }
        prop.QueueFree();
    }
}