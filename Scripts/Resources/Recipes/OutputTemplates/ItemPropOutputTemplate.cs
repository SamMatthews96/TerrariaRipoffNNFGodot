using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPropOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }

    [Export] public Array<PropPropertyOutputTemplate>
        PropProperties { get; private set; }

    public override ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        return new ItemProp(this, suppliedIngredients);
    }
}