using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Placeable : WorldObject {
    public Item Item { get; private set; }
    [Export] private Sprite2D _sprite;
    public Array<PlaceableCell> PlaceableCell { get; private set; } = new();
    

    public event Action<Placeable> Destroyed;

    public new static Placeable Create(Dictionary data) {
        throw new NotImplementedException();
    }

    public override void _Ready() {
        Position = (Coords * Game.BlockSize).ToVector2();
    }

    public void RegisterCell(PlaceableCell placeableCell) {
        PlaceableCell.Add(placeableCell);
        placeableCell.Gathered += OnPlaceableCellGathered;
    }

    private void OnPlaceableCellGathered() {
        Destroyed?.Invoke(this);
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