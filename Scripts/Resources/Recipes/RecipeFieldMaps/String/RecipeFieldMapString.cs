using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class RecipeFieldMapString : Resource {
    public abstract string ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients);
}