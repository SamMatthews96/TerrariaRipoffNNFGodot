using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemBlock : ItemProperty {
    public override PropertyType PropertyType => PropertyType.Block;
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture2D Texture { get; private set; }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath", ResourcePath);
        return serialized;
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("Weight", Weight);
        tooltipAttributes.Add("Tensile Strength", TensileStrength);
        tooltipAttributes.Add("Health", MaxHealth);
        return tooltipAttributes;
    }
}