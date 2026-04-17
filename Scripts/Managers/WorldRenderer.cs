using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public partial class WorldRenderer : Node2D {
    private const int BlockDrawDistance = 20;

    private List<IEntity>[,] _entities;
    private Player _localPlayer;
    private Vector2I _worldSize;
    private Rid _canvas;

    public static WorldRenderer Create(
        List<IEntity>[,] entities, Vector2I worldSize, Player localPlayer
    ) {
        WorldRenderer renderer = new();
        renderer._entities = entities;
        renderer._worldSize = worldSize;
        renderer._localPlayer = localPlayer;
        return renderer;
    }

    public override void _Ready() {
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(_canvas, new Transform2D(0, Vector2.Zero));
    }

    public override void _Process(double delta) {
        RenderingServer.CanvasItemClear(_canvas);

        int drawPositionXStart =
            Math.Max(0, _localPlayer.Coords.X - BlockDrawDistance);
        int drawPositionXEnd =
            Math.Min(_worldSize.X, _localPlayer.Coords.X + BlockDrawDistance);
        int drawPositionYStart =
            Math.Max(0, _localPlayer.Coords.Y - BlockDrawDistance);
        int drawPositionYEnd =
            Math.Min(_worldSize.Y, _localPlayer.Coords.Y + BlockDrawDistance);

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
    }
}