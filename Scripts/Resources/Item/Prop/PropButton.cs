using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropButton : PropProperty {
    [Export] private PackedScene _packedScene;
    
    public static PropButton Create(PackedScene packedScene) {
        return new PropButton {
            _packedScene = packedScene
        };
    }
    
    public override void Apply(ActiveProp prop, World world) {
        world.TriggerManager.Register(prop, _packedScene);
    }
}