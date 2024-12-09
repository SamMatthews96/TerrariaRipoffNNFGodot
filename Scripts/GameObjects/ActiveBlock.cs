using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock : StaticBody2D {
    public SavedBlock SavedBlock { get; private set; }
    [Export] private Dictionary _savedBlockDictionary;
    [Export] private Sprite2D _sprite;

    public override void _Ready() {
        SavedBlock = SavedBlock.FromDict(_savedBlockDictionary);

        Position = new Vector2(
            SavedBlock.XPosition * Game.BlockSize,
            SavedBlock.YPosition * Game.BlockSize);
        _sprite.Texture = SavedBlock.Item.GetProperty<Block>().Texture;
    }
}