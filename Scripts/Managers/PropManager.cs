using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

/*
 * 
 * 
 */
public partial class PropManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, Node2D> PropCells { get; private set; }

    public override void _Ready() {
        PropCells = new Dictionary<Vector2I, Node2D>();
        if (!_world.IsHost) return;
        
        _world.PlayerManager.PlayerSpawnedOnServer += OnPlayerSpawnedOnServer;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnServer -= OnPlayerSpawnedOnServer;
        };
    }
    
    private void OnPlayerSpawnedOnServer(Player player) {
        player.ActionController.BuildAction.HostPlacePropAction 
            += OnHostPlacePropAction;
        player.TreeExiting += () => {
            player.ActionController.BuildAction.HostPlacePropAction 
                -= OnHostPlacePropAction;
        };
    }

    private void OnHostPlacePropAction(Item item, Vector2I coords) {
        Prop newProp = Prop.Create(item, coords);
        foreach (Vector2I cell in newProp.Cells) {
            PropCells[cell] = newProp;
        }
        AddChild(newProp);
    }
}