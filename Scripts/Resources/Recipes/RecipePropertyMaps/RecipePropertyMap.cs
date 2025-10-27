using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public interface IRecipePropertyMap<out T> {
    public T ResolveTemplate(
        Dictionary<string, Item> suppliedIngredients);
}