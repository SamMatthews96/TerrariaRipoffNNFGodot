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
        prop.Coords = new IntVector(
            (int)Math.Round(data["xPosition"].ToString().ToFloat()),
            (int)Math.Round(data["yPosition"].ToString().ToFloat())
        );
        prop.Disable();
        return prop;
    }

    public override void _Ready() {
        Position = (Coords * Game.BlockSize).ToVector2();
        ItemBlock itemBlock = Item.GetProperty<ItemBlock>();
        _sprite.Texture = itemBlock.Texture;
        CurrentHealth = itemBlock.MaxHealth;
    }
}