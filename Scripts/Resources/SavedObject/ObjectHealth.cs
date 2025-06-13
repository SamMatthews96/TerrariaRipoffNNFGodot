using System;
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectHealth : ObjectProperty {
    [Export] public float MaxHealth { get; private set; }

    public override void OnWorldObjectCreate(WorldObject worldObject) {
        if (worldObject.TryGetProperty(out ObjectGatherable staticProperty)) {
            
        }
    }
}