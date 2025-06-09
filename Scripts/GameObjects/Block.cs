using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Block : WorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;
    public float CurrentHealth { get; set; }

    public new static Block Create(Dictionary data) {
        Item newItem = Item.FromDictionary(data["item"].AsGodotDictionary());
        IntVector coords = new(
            (int)Math.Round(data["xPosition"].ToString().ToFloat()),
            (int)Math.Round(data["yPosition"].ToString().ToFloat())
        );
        return Create(newItem, coords);
    }

    public static Block Create(Item item, IntVector coords) {
        Block block = Data.PackedScenes.ActiveBlock.Instantiate<Block>();
        block.Item = item;
        block.Coords = coords;
        block.Disable();
        return block;
    }

    public override void _Ready() {
        Position = new Vector2(Coords.X * Game.BlockSize, Coords.Y * Game.BlockSize);
        ItemBlock itemBlock = Item.GetProperty<ItemBlock>();
        _sprite.Texture = itemBlock.Texture;
        CurrentHealth = itemBlock.MaxHealth;
    }
}