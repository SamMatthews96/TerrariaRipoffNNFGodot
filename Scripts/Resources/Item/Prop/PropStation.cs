using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropStation : PropProperty {
    [Export] public StationType Type { get; private set; }

    public static PropStation Create(StationType type) {
        return new PropStation {
            Type = type
        };
    }

    public override void Apply(ActiveProp prop, World world) {
        world.StationManager.RegisterStation(Type, prop.Anchor);
    }
    
}