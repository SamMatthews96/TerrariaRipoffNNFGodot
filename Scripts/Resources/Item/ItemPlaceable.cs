using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceable : ItemProperty {
    [Export] public int Width { get; private set; }
    [Export] public int Height { get; private set; }
    [Export] public Texture2D Texture { get; private set; }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath", ResourcePath);
        return serialized;
    }

    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }
}