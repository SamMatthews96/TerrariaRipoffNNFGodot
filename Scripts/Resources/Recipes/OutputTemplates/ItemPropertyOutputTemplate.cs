using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemPropertyOutputTemplate : Resource {
    public abstract ItemProperty Build(
        Dictionary<string, Item> suppliedIngredients
    );
}