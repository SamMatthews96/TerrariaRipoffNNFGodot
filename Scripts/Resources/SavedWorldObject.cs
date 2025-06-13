using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SavedWorldObject : Resource {
    public Array<WorldObjectProperty> Properties = new();
}