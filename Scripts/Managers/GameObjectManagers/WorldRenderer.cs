using System;
using Godot;
using TerrariaRipoffNNF.TestScenes;

namespace TerrariaRipoffNNF;

public partial class WorldRenderer : Node2D {
    private const int DrawDistance = 20;

    [Export] private World _world;
    private Player _localPlayer;
    private Rid _canvas;
    
    private PlayerManager _playerManager;
    private BlockManager _blockManager;
    private ItemIdBimap _itemIdBimap;
    private Vector2I _worldSize;

    public override void _Ready() {
        _playerManager = _world.PlayerManager;
        _blockManager = _world.BlockManager;
        _itemIdBimap = _world.ItemIdBimap;
        _worldSize = _world.WorldSize;
        
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(
            _canvas, new Transform2D(0, Vector2.Zero));

        _playerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        TreeExiting += () => {
            RenderingServer.FreeRid(_canvas);
            _playerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        };
    }


    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;

        Action<Vector2I> onCreated = _ => UpdateView();
        Action<Vector2I, ushort> onDestroyed = (_, _) => UpdateView();
        CellMovedDelegate onMoved = (_, _) => UpdateView();

        _blockManager.BlockCreated += onCreated;
        _blockManager.BlockDestroyed += onDestroyed;
        _blockManager.WallCreated += onCreated;
        _blockManager.WallDestroyed += onDestroyed;
        player.CellMoved += onMoved;
        TreeExiting += () => {
            _blockManager.BlockCreated -= onCreated;
            _blockManager.BlockDestroyed -= onDestroyed;
            _blockManager.WallCreated -= onCreated;
            _blockManager.WallDestroyed -= onDestroyed;
            player.CellMoved -= onMoved;
        };
        UpdateView();
    }

    private void UpdateView() {
        RenderingServer.CanvasItemClear(_canvas);
        int playerX = _localPlayer.Coords.X;
        int playerY = _localPlayer.Coords.Y;
        int drawPositionXStart = Math.Max(0, playerX - DrawDistance);
        int drawPositionXEnd = Math.Min(_worldSize.X, playerX + DrawDistance);
        int drawPositionYStart = Math.Max(0, playerY - DrawDistance);
        int drawPositionYEnd = Math.Min(_worldSize.Y, playerY + DrawDistance);

        for (int x = drawPositionXStart; x < drawPositionXEnd; x++) {
            for (int y = drawPositionYStart; y < drawPositionYEnd; y++) {
                bool isBlock = true;
                Block? value = _blockManager.GetBlock(x, y);
                if (value is null) {
                    isBlock = false;
                    value = _blockManager.GetWall(x, y);
                }

                if (value is not {} block) continue;

                Rect2 drawDimensions = new(
                    x * Game.BlockSize,
                    y * Game.BlockSize,
                    Game.BlockSize,
                    Game.BlockSize
                );
                Item item = _itemIdBimap.GetItem(block.ItemId);
                Color color = isBlock
                    ? new Color(1, 1, 1)
                    : new Color(0.3f, 0.3f, 0.3f);

                RenderingServer.CanvasItemAddTextureRect(
                    _canvas,
                    drawDimensions,
                    item.IconTexture.GetRid(),
                    modulate: color
                );
            }
        }
    }
}