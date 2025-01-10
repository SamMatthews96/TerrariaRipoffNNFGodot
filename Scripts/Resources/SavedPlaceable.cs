using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class SavedPlaceable : Resource {
    
    public int XLeftPosition { get; private init; }
    public int YBottomPosition { get; private init; }
    public Item Item { get; private init; }
    public float CurrentHealth { get; set; }
    
    public List<IntVector> OccupiedCells { get; private init; }
    
}