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
        Array<PropProperty> newPropProperties = new();
        foreach (PropPropertyOutputTemplate template in PropProperties) {
            newPropProperties.Add(template.Build(suppliedIngredients));
        }
        return new ItemProp(Texture, Dimensions, newPropProperties);
    }
}