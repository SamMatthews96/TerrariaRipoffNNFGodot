using Godot;

namespace TerrariaRipoffNNF;

public class ObjectTexture : ObjectProperty {
    private readonly Texture2D _texture;
    private bool _isWall;
    
    public ObjectTexture(
        WorldObject worldObject, Texture2D texture2D, bool isWall = false)
        : base(worldObject) {
        _texture = texture2D;
        _isWall = isWall;
    }

    public override void Init() {
        Sprite2D sprite = new();
        sprite.Scale = WorldObject.HasProperty<ObjectCanPickup>()
            ? new Vector2(0.2f, 0.2f)
            : new Vector2(0.25f, 0.25f);
        sprite.Texture = _texture;
        if (_isWall) {
            sprite.Modulate = new Color(0.7f, 0.7f, 0.7f);
        }
        WorldObject.ParentNode.AddChild(sprite);
    }
}