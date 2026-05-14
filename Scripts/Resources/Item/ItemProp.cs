using System.Linq;
using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

using ObjectDictionary = System.Collections.Generic.Dictionary<string, object>;

[GlobalClass]
public partial class ItemProp : ItemProperty {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }
    [Export] public bool DoesDropSelf { get; private set; } = true;
    [Export] public Array<PropProperty> Properties = new();

    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public T GetProperty<T>() where T : PropProperty {
        foreach (PropProperty prop in Properties) {
            if (prop is T typedProp) {
                return typedProp;
            }
        }

        throw new InvalidOperationException(
            $"Property of type {typeof(T).Name} not found");
    }

    public bool TryGetProperty<T>(out T property) where T : PropProperty {
        foreach (PropProperty prop in Properties) {
            if (prop is not T typedProp) continue;
            property = typedProp;
            return true;
        }

        property = null;
        return false;
    }

    public bool HasProperty<T>() where T : PropProperty {
        return Properties.OfType<T>().Any();
    }

    public ItemProp(
        Texture2D texture, Vector2I dimensions,
        Array<PropProperty> properties = null
    ) {
        Texture = texture;
        Dimensions = dimensions;
        Properties = properties;
    }

    public ItemProp(
        ItemPropOutputTemplate template,
        Dictionary<string, Item> suppliedIngredients
    ) {
        Texture = template.Texture;
        Dimensions = template.Dimensions;
        foreach (PropPropertyOutputTemplate propTemplate in
                 template.PropProperties) {
            PropProperty prop = propTemplate.Build(suppliedIngredients);
            Properties.Add(prop);
        }
    }

    public ItemProp() { }
}