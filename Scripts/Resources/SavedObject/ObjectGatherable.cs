using System;
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectGatherable : ObjectProperty {
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        worldObject.ActiveProperties
            .Add(new ActiveObjectGatherable(worldObject));
    }
}