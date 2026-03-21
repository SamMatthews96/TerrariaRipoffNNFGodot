using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class RenderingServerTest : Node2D {
    [Export] private Texture _texture;
    
    private Rid _canvas;
    private Rid _textureRid;

    private List<TestItems> _itemsToDraw;
    
    private const float Size = 32;
    
    public override void _Ready() {
        _canvas = RenderingServer.CanvasItemCreate();
        RenderingServer.CanvasItemSetParent(_canvas, GetCanvasItem());
        RenderingServer.CanvasItemSetTransform(_canvas, new Transform2D(0, Vector2.Zero)); 
        _textureRid = _texture.GetRid();

        _itemsToDraw = new List<TestItems> {
            new TestItems { Position = new Vector2(0, 0) },
            new TestItems { Position = new Vector2(1, 0) },
            new TestItems {Position = new Vector2(0, 1)},
            new TestItems {Position = new Vector2(1, 1)},
            new TestItems {Position = new Vector2(2, 2)}
        };
    }

    public override void _Process(double delta) {
        RenderingServer.CanvasItemClear(_canvas);

        foreach (TestItems testItems in _itemsToDraw) {
            Rect2 drawDimensions = new(
                testItems.Position.X * Size,
                testItems.Position.Y * Size, 
                Size, 
                Size
            );
            
            RenderingServer.CanvasItemAddTextureRect(
                _canvas, 
                drawDimensions, 
                _textureRid
            );
        }
        
    }
}

public struct TestItems {
    public Vector2 Position;
}