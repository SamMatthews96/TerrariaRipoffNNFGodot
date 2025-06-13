using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class SavedObjects : Resource {
    [Export] public SavedObject Stone { get; private set; }
    [Export] public SavedObject Earth { get; private set; }
}

