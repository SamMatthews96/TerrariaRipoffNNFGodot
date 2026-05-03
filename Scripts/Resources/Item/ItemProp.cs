using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemProp : ItemProperty {
    [Export] public Texture2D Texture { get; private set; }
    [Export] public Vector2I Dimensions { get; private set; }
    [Export] public bool DoesDropSelf { get; private set; } = true;
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemProp(Texture2D texture, Vector2I dimensions) {  
        Texture = texture;
        Dimensions = dimensions;
    }

    public ItemProp() { }
}