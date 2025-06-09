using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Placeable : WorldObject {
    public Item Item { get; private set; }
    public Texture2D Texture;
    
    public new static Placeable Create(Dictionary data) {
        throw new NotImplementedException();
    }
    
    public static Placeable Create(Item item, IntVector coords) {
        Placeable placeable = Data.PackedScenes.ActivePlaceable.Instantiate<Placeable>();
        placeable.Item = item;
        placeable.Coords = coords;
        placeable.Texture = item.GetProperty<ItemPlaceable>().Texture;
        placeable.Disable();
        return placeable;
    }
    
}