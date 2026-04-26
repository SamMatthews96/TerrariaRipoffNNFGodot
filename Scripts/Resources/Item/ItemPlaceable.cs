using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceable : ItemProperty {
    [Export] public PlaceableType Type { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemPlaceable(Texture2D texture, Vector2I dimensions, PlaceableType type) { 
        Texture = texture;
        Dimensions = dimensions;
        Type = type;
    }

    public ItemPlaceable() { }
}