using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeIngredient : Resource {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public int Amount { get; private set; }
}