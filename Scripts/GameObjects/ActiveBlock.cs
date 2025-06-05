using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock : ActiveWorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;
    public float CurrentHealth { get; set; }

    public new static ActiveBlock Create(Dictionary data) {
        ActiveBlock activeBlock = Data.PackedScenes.ActiveBlock.Instantiate<ActiveBlock>();
        activeBlock.Item = Item.FromDictionary(data["item"].AsGodotDictionary());

        activeBlock.XPosition = (int)Math.Round(data["xPosition"].ToString().ToFloat());
        activeBlock.YPosition = (int)Math.Round(data["yPosition"].ToString().ToFloat());
        activeBlock.Disable();
        return activeBlock;
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