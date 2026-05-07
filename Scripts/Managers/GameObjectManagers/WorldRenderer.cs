using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldRenderer : Node2D {
    private const int DrawDistance = 20;

    [Export] private World _world;
    private Player _localPlayer;
    private Rid _canvas;

    public override void _Ready() {
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(
            _canvas, new Transform2D(0, Vector2.Zero));

        _world.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        TreeExiting += () => {
            RenderingServer.FreeRid(_canvas);
            _world.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        };
    }


    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;

        Action<Vector2I> onCreated = _ => UpdateView();
        Action<Vector2I, ushort> onDestroyed = (_, _) => UpdateView();
        Player.CellMovedDelegate onMoved = (_, _) => UpdateView();

        _world.BlockManager.BlockCreated += onCreated;
        _world.BlockManager.BlockDestroyed += onDestroyed;
        _world.BlockManager.WallCreated += onCreated;
        _world.BlockManager.WallDestroyed += onDestroyed;
        player.MovedCellLocal += onMoved;
        TreeExiting += () => {
            _world.BlockManager.BlockCreated -= onCreated;
            _world.BlockManager.BlockDestroyed -= onDestroyed;
            _world.BlockManager.WallCreated -= onCreated;
            _world.BlockManager.WallDestroyed -= onDestroyed;
            player.MovedCellLocal -= onMoved;
        };
        UpdateView();
    }

    private void UpdateView() {
        RenderingServer.CanvasItemClear(_canvas);
        int playerX = _localPlayer.Coords.X;
        int playerY = _localPlayer.Coords.Y;

        int drawPositionXStart = Math.Max(0, playerX - DrawDistance);
        int drawPositionXEnd = Math.Min(_world.WorldSize.X, playerX + DrawDistance);
        int drawPositionYStart = Math.Max(0, playerY - DrawDistance);
        int drawPositionYEnd = Math.Min(_world.WorldSize.Y, playerY + DrawDistance);

        for (int x = drawPositionXStart; x < drawPositionXEnd; x++) {
            for (int y = drawPositionYStart; y < drawPositionYEnd; y++) {
                bool isBlock = true;
                Block? value = _world.BlockManager.Blocks[x, y];
                if (value is null) {
                    isBlock = false;
                    value = _world.BlockManager.Walls[x, y];
                }

                if (value is not {} block) continue;

                Rect2 drawDimensions = new(
                    x * Game.BlockSize,
                    y * Game.BlockSize,
                    Game.BlockSize,
                    Game.BlockSize
                );
                Item item = _world.ItemIdBimap.GetItem(block.ItemId);
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