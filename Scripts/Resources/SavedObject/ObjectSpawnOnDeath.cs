using System;
using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectSpawnOnDeath : ObjectProperty {
    [Export] public SavedObject SavedObject { get; private set; }
    
    public override void Register(WorldObject worldObject) {
        worldObject.ActiveProperties
            .Add(new ActiveObjectSpawnOnDeath(worldObject));
    }
}