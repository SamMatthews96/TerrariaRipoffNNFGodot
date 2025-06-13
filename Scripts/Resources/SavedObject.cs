using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class SavedObject : Resource {
    [Export] public Array<ObjectProperty> Properties = new();
}