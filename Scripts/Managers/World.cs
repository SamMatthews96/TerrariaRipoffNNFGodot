using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

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

    public const int BlockSize = 32;
    
    public event Action WorldLoadedLocally;
    public event Action WorldSaved;

    public void SetGameAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;

        _entities = new List<IEntity>[_game.Width, _game.Height];
        for (int x = 0; x < _game.Width; x++) {
            for (int y = 0; y < _game.Height; y++) {
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
            
        _game.Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
    }

    public override void _Ready() {
        // init canvas
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(_canvas, new Transform2D(0, Vector2.Zero)); 
        
    }

    private Godot.Collections.Dictionary<string, Rid> _textures;
    
    public override void _Process(double delta) {
        // draw the world?
        RenderingServer.CanvasItemClear(_canvas);
        
        int drawPositionXStart = 
            Math.Max(0, _localPlayer.Coords.X - _blockDrawDistance);
        int drawPositionXEnd = 
            Math.Min(_game.Width, _localPlayer.Coords.X + _blockDrawDistance);
        int drawPositionYStart = 
            Math.Max(0, _localPlayer.Coords.Y - _blockDrawDistance);
        int drawPositionYEnd = 
            Math.Min(_game.Height, _localPlayer.Coords.Y + _blockDrawDistance);

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
        // Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnExitGameClicked() {
        Visible = false;
        _game.Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked;
        
  
    }
    
    public void SetGameAsClient(Game game, Dictionary playerData) {
        if (_game is not null) throw new Exception("[20250529.2332.1] Game already set");
        _game = game;
        _localPlayerData = playerData;
        // if the playerData contains a defaultSpawnPosition, we use that
        // otherwise, we need to get the information from the server
        // before we spawn the player

        // RpcId(SceneManager.HostId, nameof(CmdRequestWorldData),
        //     _game.PeerId, _defaultSpawnPosition.x, _defaultSpawnPosition.y);
    }

    private Player SpawnLocalPlayer() {
        Player player = Player.Create(_game.PeerId, new IntVector(5, 5));
        player.InitAsLocal(_game, _localPlayerData);
        AddChild(player, true);
        _players.Add(_game.PeerId, player);

        Rpc(nameof(RpcOnNewPlayerJoining), _game.PeerId);
        return player;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RpcOnNewPlayerJoining(int newPeerId) {
        Player player = Player.Create(newPeerId, new IntVector(5, 5));
        AddChild(player, true);
        _players.Add(newPeerId, player);

        _players[_game.PeerId].AddPeerToSynchronizer(newPeerId);

        RpcId(newPeerId, nameof(SpawnRemoteExistingPlayer), _game.PeerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void SpawnRemoteExistingPlayer(int peerId) {
        Player player = Player.Create(peerId,
            new IntVector(5, 5));
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

}