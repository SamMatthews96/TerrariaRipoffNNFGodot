using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class CraftStationArea : Area2D {
    public ObjectCraftStation CraftStation { get; private set; }

    public static CraftStationArea Create(ObjectCraftStation craftStation) {
        CraftStationArea newArea =
            Data.PackedScenes.CraftStationArea.Instantiate<CraftStationArea>();
        newArea.CraftStation = craftStation;
        return newArea;
    }
}