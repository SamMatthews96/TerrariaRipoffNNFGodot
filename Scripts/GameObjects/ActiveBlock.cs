using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock : ActiveWorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;

    public override void _Ready() {
        Position = new Vector2(
            XPosition * Game.BlockSize,
            YPosition * Game.BlockSize);
        _sprite.Texture = Item.GetProperty<ItemBlock>().Texture;
    }

    public new static ActiveBlock Create(Dictionary data) {
        ActiveBlock activeBlock = Data.PackedScenes.ActiveBlock.Instantiate<ActiveBlock>();
        activeBlock.Item = Item.FromDictionary(data["item"].AsGodotDictionary());

        return activeBlock;
    }
}