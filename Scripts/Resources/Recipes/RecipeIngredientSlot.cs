using Godot;
using TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeIngredientSlot : Resource {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public int Amount { get; private set; }
    [Export] public Texture2D Icon { get; private set; }
    [Export] public bool Required { get; private set; }
}