using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class Recipe : Resource {
    
    [Export] public Array<RecipeIngredient> Ingredients { get; private set; } = new();
    
    // result
}