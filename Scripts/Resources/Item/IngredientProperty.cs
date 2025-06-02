using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class IngredientProperty : Resource {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public float Quality { get; private set; }
    [Export] public string Name { get; private set; }
}