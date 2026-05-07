using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropTrigger : PropProperty {
    [Export] private PackedScene _packedScene;
    
    public static PropTrigger Create(PackedScene packedScene) {
        return new PropTrigger {
            _packedScene = packedScene
        };
    }
    
    public override void Apply(ActiveProp prop, World world) {
        world.TriggerManager.RegisterTrigger(prop, _packedScene);
    }
}