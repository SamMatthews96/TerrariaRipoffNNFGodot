using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

/*
    reworking the node-based world was necessary
    creating a node for each block was too much overhead
        
    But now we need to recreate the lost functionality
        blocks preventing movement
            idea 2: manually implement a collision system
                // the collision logic would be a little bit complex
                // but potentially much more performant and maintainable
            
        blocks being clickable to perform actions
            we sort of already had this
 */

public partial class World : Node2D {
    private Game _game;
    private List<IEntity>[,] _entities;
    private string _worldName;

    private (int x, int y) _defaultSpawnPosition;

    private Dictionary _localPlayerData;
    private Player _localPlayer;
    private int _blockDrawDistance = 20;

    private Godot.Collections.Dictionary<int, Player> _players = new();

    private Rid _canvas;

    [Export] private PackedScene _collisionBlock;
    private StaticBody2D[,] _activeCollisionBlocks;
    
    public Vector2I WorldSize { get; private set; }

    public event Action WorldLoadedLocally;
    public event Action WorldSaved;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;
        
        WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        _activeCollisionBlocks = new StaticBody2D[WorldSize.X, WorldSize.Y];

        _entities = new List<IEntity>[WorldSize.X, WorldSize.Y];
        for (int x = 0; x < WorldSize.X; x++) {
            for (int y = 0; y < WorldSize.Y; y++) {
                _entities[x, y] = new List<IEntity>();
            }
        }

        _worldName = worldData["Name"].ToString();
        Array defaultSpawnPos = worldData["DefaultSpawnPosition"].AsGodotArray();
        _defaultSpawnPosition =
            ((int)defaultSpawnPos[0], (int)defaultSpawnPos[1]);

        Array allWorldObjects = worldData["SavedWorldObjects"].AsGodotArray();
        foreach (Dictionary dictionary in allWorldObjects) {
            int x = (int)dictionary["xPosition"].ToString().ToFloat();
            int y = (int)dictionary["yPosition"].ToString().ToFloat();

            IEntity entity;
            switch (dictionary["type"].ToString()) {
                case "block":
                    entity = new BlockEntity() {
                        CellCoordinates = new Vector2(x,y),
                        CurrentHealth = 1,
                        ResourcePath = dictionary["item"].AsGodotDictionary()["ResourcePath"].ToString(),
                    };
                    break;
                default:
                    throw new Exception(
                        $"[20250529.2332.1] Unknown world object type: {dictionary["type"].ToString()}");
            }
            
            _entities[x ,y].Add(entity); 
        }
        
        WorldLoadedLocally?.Invoke();
        _localPlayer = SpawnLocalPlayer();
        _localPlayer.MovedCell += OnLocalPlayerMovedCell;
            
        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    #region Draw World
    public override void _Ready() {
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(_canvas, new Transform2D(0, Vector2.Zero)); 
    }

    public override void _Process(double delta) {
        RenderingServer.CanvasItemClear(_canvas);
        
        int drawPositionXStart = 
            Math.Max(0, _localPlayer.Coords.X - _blockDrawDistance);
        int drawPositionXEnd = 
            Math.Min(WorldSize.X, _localPlayer.Coords.X + _blockDrawDistance);
        int drawPositionYStart = 
            Math.Max(0, _localPlayer.Coords.Y - _blockDrawDistance);
        int drawPositionYEnd = 
            Math.Min(WorldSize.Y, _localPlayer.Coords.Y + _blockDrawDistance);

        for (int x = drawPositionXStart; x < drawPositionXEnd; x++) {
            for (int y = drawPositionYStart; y < drawPositionYEnd; y++) {
                List<IEntity> cellEntities = _entities[x, y];
                foreach (IEntity entity in cellEntities) {
                    if (entity is BlockEntity blockEntity) {
                        Rect2 drawDimensions = new(
                            blockEntity.CellCoordinates.X * Game.BlockSize,
                            blockEntity.CellCoordinates.Y * Game.BlockSize, 
                            Game.BlockSize, 
                            Game.BlockSize
                        );
                        Item item = ResourceLoader.Load<Item>(blockEntity.ResourcePath);
                    
                        RenderingServer.CanvasItemAddTextureRect(
                            _canvas,
                            drawDimensions, 
                            item.IconTexture.GetRid()
                        );
                    }
                }
            }
        }
    }

    public override void _ExitTree() {
        RenderingServer.FreeRid(_canvas);
        
        _localPlayer.MovedCell -= OnLocalPlayerMovedCell;
        
    }

    #endregion
    
    private void OnExitGameClicked() {
        Visible = false;
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
    }
    
    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;
    }

    private Player SpawnLocalPlayer() {
        Player player = Player.Create(_game.PeerId, new Vector2I(10, 14));
        player.InitAsLocal(_game, _localPlayerData);
        AddChild(player, true);
        _players.Add(_game.PeerId, player);
        player.ActionController.GatherAction.GatherAttempted += OnPlayerGatherAttempted;

        Rpc(nameof(RpcOnNewPlayerJoining), _game.PeerId);
        return player;
    }

    private void OnPlayerGatherAttempted(Vector2I coords, Player player) {
        List<IEntity> cellEntities = _entities[coords.X, coords.Y];
        
        for (int i = 0; i < cellEntities.Count; i++) {
            IEntity entity = cellEntities[i];
            if (entity is BlockEntity blockEntity) {
                blockEntity.CurrentHealth -= player.PlayerEquipment.Pickaxe.Power;
                if (blockEntity.CurrentHealth <= 0) {
                    cellEntities.RemoveAt(i);
                }
                break;
            }
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcOnNewPlayerJoining(int newPeerId) {
        Player player = Player.Create(newPeerId, new Vector2I(5, 5));
        AddChild(player, true);
        _players.Add(newPeerId, player);

        _players[_game.PeerId].AddPeerToSynchronizer(newPeerId);

        RpcId(newPeerId, nameof(SpawnRemoteExistingPlayer), _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SpawnRemoteExistingPlayer(int peerId) {
        Player player = Player.Create(peerId, new Vector2I(5, 5));
        AddChild(player, true);
        _players.Add(peerId, player);
    }

    [Rpc(CallLocal = true)]
    private void RpcGatherSuccess() {
        Player player = _players[_game.PeerId];
        player.ActionController.GatherAction.OnAfterGatherSuccess();
    }

    [Rpc(CallLocal = true)]
    private void RpcBuildSuccess(Dictionary data) {
        Item item = Item.FromDictionary(data);
        Player player = _players[_game.PeerId];
        player.Inventory.OnAfterBuildSuccess(item);
    }
    
    public bool IsInBounds(Vector2I intVector) {
        return intVector.X >= 0
               && intVector.X < WorldSize.X
               && intVector.Y >= 0
               && intVector.Y < WorldSize.Y;
    }

    private void OnLocalPlayerMovedCell(Vector2I playerPosition) {
        int radius = 3;
        int startX = Mathf.Max(0, playerPosition.X - radius);
        int endX = Mathf.Min(WorldSize.X - 1, playerPosition.X + radius);
        int startY = Mathf.Max(0, playerPosition.Y - radius);
        int endY = Mathf.Min(WorldSize.Y - 1, playerPosition.Y + radius);

        // Create collision blocks within radius where blocks exist
        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                if (_activeCollisionBlocks[x, y] == null && HasBlockEntity(x, y)) {
                    CreateCollisionBlock(x, y);
                }
            }
        }
    }

    private bool HasBlockEntity(int x, int y) {
        List<IEntity> entities = _entities[x, y];
        if (entities == null) return false;

        foreach (IEntity entity in entities) {
            if (entity is BlockEntity) {
                return true;
            }
        }

        return false;
    }

    private void CreateCollisionBlock(int x, int y) {
        var block = _collisionBlock.Instantiate<StaticBody2D>();
        block.Position = new Vector2(x * Game.BlockSize, y * Game.BlockSize);
        AddChild(block);

        _activeCollisionBlocks[x, y] = block;
    }

    // private void RemoveCollisionBlockAt(int x, int y) {
    //     if (x >= 0 && x < _activeCollisionBlocks.GetLength(0) &&
    //         y >= 0 && y < _activeCollisionBlocks.GetLength(1) &&
    //         _activeCollisionBlocks[x, y] != null) {
    //         _activeCollisionBlocks[x, y].QueueFree();
    //         _activeCollisionBlocks[x, y] = null;
    //     }
    // }

    // private void ClearAllCollisionBlocks() {
    //     for (int x = 0; x < _activeCollisionBlocks.GetLength(0); x++) {
    //         for (int y = 0; y < _activeCollisionBlocks.GetLength(1); y++) {
    //             if (_activeCollisionBlocks[x, y] != null) {
    //                 _activeCollisionBlocks[x, y].QueueFree();
    //                 _activeCollisionBlocks[x, y] = null;
    //             }
    //         }
    //     }
    // }
}