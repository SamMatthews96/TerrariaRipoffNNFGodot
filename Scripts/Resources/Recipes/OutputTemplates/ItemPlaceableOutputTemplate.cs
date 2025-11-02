using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceableOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Array<IntVector> OccupiedCells { get; private set; } 
    [Export] public PlaceableType Type { get; private set; }
    

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        return new ItemPlaceable(Texture, OccupiedCells,Type);
    }
}