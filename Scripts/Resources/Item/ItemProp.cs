using System.Linq;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemProp : ItemProperty {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }
    [Export] public bool DoesDropSelf { get; private set; } = true;
    [Export] public Array<PropProperty> Props { get; private set; } = new();
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public T GetProperty<T>() where T : PropProperty {
        foreach (PropProperty prop in Props) {
            if (prop is T typedProp) {
                return typedProp;
            }
        }
        throw new System.InvalidOperationException($"Property of type {typeof(T).Name} not found");
    }

    public bool TryGetProperty<T>(out T property) where T : PropProperty {
        foreach (PropProperty prop in Props) {
            if (prop is not T typedProp) continue;
            property = typedProp;
            return true;
        }
        property = null;
        return false;
    }

    public bool HasProperty<T>() where T : PropProperty {
        return Props.OfType<T>().Any();
    }

    public ItemProp(Texture2D texture, Vector2I dimensions) {
        Texture = texture;
        Dimensions = dimensions;
    }

    public ItemProp() { }
}