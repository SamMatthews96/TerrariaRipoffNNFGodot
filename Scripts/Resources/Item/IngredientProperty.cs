using Godot;

namespace TerrariaRipoffNNF;

public enum IngredientType {
    Wood,
    StrongMetal,
    PreciousMetal,
    ConductiveMetal,
    Stone,
}

[GlobalClass]
public sealed partial class IngredientProperty : Resource {

    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public float Quality { get; private set; }
}