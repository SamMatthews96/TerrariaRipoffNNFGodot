using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceable : ItemProperty {
    [Export] public PlaceableType Type { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<Vector2I> OccupiedCells { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemPlaceable(Texture2D texture, Array<Vector2I> occupiedCells, 
        PlaceableType type) {
        Texture = texture;
        OccupiedCells = occupiedCells;
        Type = type;
    }

    public ItemPlaceable() { }
}