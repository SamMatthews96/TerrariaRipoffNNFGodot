using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlaceableCell : WorldObject {
    public Placeable Placeable { get; private set; }
    public event Action Gathered;

    public static PlaceableCell Create(Placeable placeable, IntVector coords) {
        PlaceableCell placeableCell =
            Data.PackedScenes.PlaceableCell.Instantiate<PlaceableCell>();
        placeableCell.Placeable = placeable;
        placeableCell.Coords = coords;
        return placeableCell;
    }

    public void OnGather() {
        Gathered?.Invoke();
    }
}