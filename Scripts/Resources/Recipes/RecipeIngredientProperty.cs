using Godot;
using TerrariaRipoffNNF;

[GlobalClass]
public partial class RecipeIngredientProperty : Resource {
    [Export] public IngredientType IngredientType { get; private set; }
    [Export] public int Amount { get; private set; }
    public Item SelectedItem { get; private set; }

    // 
}