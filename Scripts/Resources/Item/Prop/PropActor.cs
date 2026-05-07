using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropActor : PropProperty {
    [Export] private PackedScene _packedScene;
    public static PropActor Create(PackedScene packedScene) {
        return new PropActor {
            _packedScene = packedScene
        };
    }

    public override void Apply(ActiveProp prop, World world) {
        world.TriggerManager.RegisterActor(prop, _packedScene);
    }
}