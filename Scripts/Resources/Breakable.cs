using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Breakable : Resource {
    [Export] public int Id { get; private set; }
    [Export] public Item Item { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    
}