using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class PlaceableProperty : Resource {
    [Export] public Array<IntVector> OccupiedCells;
}