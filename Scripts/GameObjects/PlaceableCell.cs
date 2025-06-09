using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlaceableCell : WorldObject {
    private Placeable _placeable;

    public override void _Ready() {
        
    }

    public static PlaceableCell Create(Placeable placeable, IntVector coords) {
        PlaceableCell placeableCell =
            Data.PackedScenes.ActivePlaceable.Instantiate<PlaceableCell>();
        placeableCell.Coords = coords;
        placeableCell._placeable = placeable;
        return placeableCell;
    }
}