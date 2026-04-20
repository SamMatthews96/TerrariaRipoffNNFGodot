using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class WorldRenderer : Node2D {
    private const int BlockDrawDistance = 20;

    [Export] private World _world;
    private Block[,] _blocks;
    private Player _localPlayer;
    private Vector2I _worldSize;
    private Rid _canvas;
    
    public override void _Ready() {
        _blocks = _world.Blocks;
        _worldSize = _world.WorldSize;
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(_canvas, new Transform2D(0, Vector2.Zero));
        
        _world.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta) {
        RenderingServer.CanvasItemClear(_canvas);
        int playerX = _localPlayer.Coords.X;
        int playerY = _localPlayer.Coords.Y;
        int drawPositionXStart = Math.Max(0, playerX - BlockDrawDistance);
        int drawPositionXEnd = Math.Min(_world.WorldSize.X, playerX + BlockDrawDistance);
        int drawPositionYStart = Math.Max(0, playerY - BlockDrawDistance);
        int drawPositionYEnd = Math.Min(_world.WorldSize.Y, playerY + BlockDrawDistance);

        for (int x = drawPositionXStart; x < drawPositionXEnd; x++) {
            for (int y = drawPositionYStart; y < drawPositionYEnd; y++) {
                Block block = _world.Blocks[x, y];
                if (block == null) continue;
                Rect2 drawDimensions = new(
                    x * Game.BlockSize,
                    y * Game.BlockSize,
                    Game.BlockSize,
                    Game.BlockSize
                );
                Item item = ResourceLoader.Load<Item>(block.ResourcePath);

                RenderingServer.CanvasItemAddTextureRect(
                    _canvas,
                    drawDimensions,
                    item.IconTexture.GetRid()
                );
            }
        }
    }

    public override void _ExitTree() {
        RenderingServer.FreeRid(_canvas);
        _world.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }
}