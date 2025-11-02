using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class RecipeFieldMapFloat : Resource {
    public abstract float ResolveTemplate(Dictionary<string, Item> suppliedIngredients);
}