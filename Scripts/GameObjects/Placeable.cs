using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Placeable : WorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;
    
    public new static Placeable Create(Dictionary data) {
        throw new NotImplementedException();
    }

    public override void _Ready() {
        Position = (Coords * Game.BlockSize).ToVector2();
    }
    
    public static Placeable Create(Item item, IntVector coords) {
        Placeable placeable = Data.PackedScenes.Placeable.Instantiate<Placeable>();
        placeable.Item = item;
        placeable.Coords = coords;
        placeable._sprite.Texture = item.GetProperty<ItemPlaceable>().Texture;
        placeable.Disable();
        return placeable;
    }
    
}