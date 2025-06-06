using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Block : WorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;
    public float CurrentHealth { get; set; }

    public new static Block Create(Dictionary data) {
        Block block = Data.PackedScenes.ActiveBlock.Instantiate<Block>();
        block.Item = Item.FromDictionary(data["item"].AsGodotDictionary());

        block.XPosition = (int)Math.Round(data["xPosition"].ToString().ToFloat());
        block.YPosition = (int)Math.Round(data["yPosition"].ToString().ToFloat());
        block.Disable();
        return block;
    }

    public override void _Ready() {
        Position = new Vector2(
            XPosition * Game.BlockSize,
            YPosition * Game.BlockSize);
        ItemBlock itemBlock = Item.GetProperty<ItemBlock>();
        _sprite.Texture = itemBlock.Texture;
        CurrentHealth = itemBlock.MaxHealth;
    }
}