using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class PropPropertyOutputTemplate : Resource {
    public abstract PropProperty Build(
        Dictionary<string, Item> suppliedIngredients
    );
}