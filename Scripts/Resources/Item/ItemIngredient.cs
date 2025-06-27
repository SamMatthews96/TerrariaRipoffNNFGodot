using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemIngredient : ItemProperty {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public float Quality { get; private set; }
    [Export] public string Name { get; private set; }

    public static ItemIngredient Create(
        IngredientType type, float quality, string name
    ) {
        return new ItemIngredient {
            IngredientType = type,
            Quality = quality,
            Name = name
        };
    }

    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }
}