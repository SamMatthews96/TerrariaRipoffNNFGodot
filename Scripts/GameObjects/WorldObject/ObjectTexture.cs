using Godot;

namespace TerrariaRipoffNNF;

public class ObjectTexture : ObjectProperty {
    private readonly Texture2D _texture;
    public ObjectTexture(WorldObject worldObject, Texture2D texture2D)
        : base(worldObject) {
        _texture = texture2D;
    }
    public override void Init() {
        Sprite2D sprite = new();
        sprite.Scale = WorldObject.HasProperty<ObjectCanPickup>() 
            ? new Vector2(0.2f, 0.2f) 
            : new Vector2(0.25f, 0.25f);
        sprite.Texture = _texture;
        WorldObject.ParentNode.AddChild(sprite);
    }
}