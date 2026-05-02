using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Prop : Node2D {
    public Item Item { get; protected set; }
    public Array<Vector2I> Cells { get; private set; } = new();
    [Export] protected Sprite2D Sprite;
    
}