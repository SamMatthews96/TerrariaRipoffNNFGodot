using Godot;

namespace TerrariaRipoffNNF;

public enum IngredientType {
    StrongMetal, Wood, Stone,
}

[GlobalClass]
public partial class IngredientProperty : Resource {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public float Quality { get; private set; }
}