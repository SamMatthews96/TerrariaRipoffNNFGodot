using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ObjectTexture : ObjectProperty {
    [Export] public Texture2D Texture { get; private set; }
    public override void OnWorldObjectCreate(WorldObject worldObject) {
        Sprite2D sprite = new();
        sprite.Scale = new Vector2(0.25f, 0.25f);
        sprite.Texture = Texture;
        worldObject.AddChild(sprite);
    }
}