using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Prop : WorldObject {
    public Item Item { get; private set; }
    public float CurrentHealth { get; set; }

    [Export] private Sprite2D _sprite;

    public new static Prop Create(Dictionary data) {
        Prop prop = Data.PackedScenes.ActiveProp.Instantiate<Prop>();
        prop.Item = Item.FromDictionary(data["item"].AsGodotDictionary());

        prop.XPosition = (int)Math.Round(data["xPosition"].ToString().ToFloat());
        prop.YPosition = (int)Math.Round(data["yPosition"].ToString().ToFloat());
        prop.Disable();
        return prop;
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