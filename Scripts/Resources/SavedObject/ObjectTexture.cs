using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectTexture : ObjectProperty {
    [Export] public Texture2D Texture { get; private set; }

    public override void Register(WorldObject worldObject) {
        worldObject.ActiveProperties
            .Add(new ActiveObjectTexture(worldObject, Texture));
    }
}