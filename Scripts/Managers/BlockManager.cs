using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BlockManager : Node2D {
    public Block[,] Blocks { get; private set; }
    public Block[,] Walls { get; private set; }

    [Export] private World _world;

    public event Action<Vector2I, string> BlockDestroyed; 
    public event Action<Vector2I> BlockCreated;

    public override void _Ready() {
        Blocks = new Block[_world.WorldSize.X, _world.WorldSize.Y];
        Walls = new Block[_world.WorldSize.X, _world.WorldSize.Y];

        if (!_world.IsHost) return;
        
        Array savedBlocks = _world.WorldData["blocks"].AsGodotArray();
        foreach (Dictionary dictionary in savedBlocks) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            Blocks[x, y] = new Block {
                CurrentHealth = 1,
                ResourcePath = dictionary["item"]
                    .AsGodotDictionary()["ResourcePath"].ToString(),
            };
        }
        
        Array savedWalls = _world.WorldData["walls"].AsGodotArray();
        foreach (Dictionary dictionary in savedWalls) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();
            
            Walls[x, y] = new Block {
                CurrentHealth = 1,
                ResourcePath = dictionary["item"]
                    .AsGodotDictionary()["ResourcePath"].ToString(),
            };
        }
        
        _world.PlayerManager.PlayerSpawnedOnHost += OnPlayerSpawnedOnHost;
        TreeExiting += () => {
            _world.PlayerManager.PlayerSpawnedOnHost -= OnPlayerSpawnedOnHost;
        };
    }

    private void OnPlayerSpawnedOnHost(Player player) {
        player.ActionController.BuildAction.HostPlaceBlockAction 
            += OnHostPlaceBlockAction;
        player.ActionController.BuildAction.HostPlaceWallAction +=
            OnHostPlaceWallAction;
        player.ActionController.GatherAction.HostGatherBlockAction +=
            OnHostGatherBlockAction;
        player.TreeExiting += () => {
            player.ActionController.BuildAction.HostPlaceBlockAction 
                -= OnHostPlaceBlockAction;
            player.ActionController.GatherAction.HostGatherBlockAction -=
                OnHostGatherBlockAction;
            player.ActionController.BuildAction.HostPlaceWallAction -=
                OnHostPlaceWallAction;
        };
    }

    private void OnHostPlaceBlockAction(Item item, Vector2I coords) {
        Rpc(nameof(RpcAllCreateBlock), item.ResourcePath, coords);
    }
    
    [Rpc(CallLocal = true)]
    private void RpcAllCreateBlock(string resourcePath, Vector2I coords) {
        Blocks[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = resourcePath
        };
        BlockCreated?.Invoke(coords);
    }
        
    
    private void OnHostPlaceWallAction(Item item, Vector2I coords) {
        Rpc(nameof(RpcAllCreateWall), item.ToDictionary(), coords);
    }

    [Rpc(CallLocal = true)]
    private void RpcAllCreateWall(Dictionary itemDict, Vector2I coords) {
        Walls[coords.X, coords.Y] = new Block {
            CurrentHealth = 1,
            ResourcePath = itemDict["ResourcePath"].ToString()
        };
    }
    
    private void OnHostGatherBlockAction(Vector2I coords, float damage) {
        Block block = Blocks[coords.X, coords.Y];
        block.CurrentHealth -= damage;
        if (block.CurrentHealth <= 0) {
            Rpc(nameof(RpcAllDestroyBlock), coords, block.ResourcePath);
        }
    }

    [Rpc(CallLocal = true)]
    private void RpcAllDestroyBlock(Vector2I coords, string resourcePath) {
        Blocks[coords.X, coords.Y] = null;
        BlockDestroyed?.Invoke(coords, resourcePath);
    }
}
