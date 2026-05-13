using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class RecipeFieldMapTexture : Resource {
    public abstract Texture2D ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients);
}