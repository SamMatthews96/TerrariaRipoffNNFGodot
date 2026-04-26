using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceableOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public PlaceableType Type { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        return new ItemPlaceable(Texture, Dimensions, Type);
    }
}