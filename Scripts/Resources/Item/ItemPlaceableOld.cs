using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemPlaceableOld : ItemProperty {
    [Export] public Array<IntVector> OccupiedCells { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemPlaceableOld(Texture2D texture, Array<IntVector> cells) {
        Texture = texture;
        OccupiedCells = cells;
    }

    public ItemPlaceableOld() { }
}