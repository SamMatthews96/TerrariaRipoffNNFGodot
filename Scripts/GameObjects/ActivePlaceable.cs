using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActivePlaceable : Node2D {
    public SavedPlaceable SavedPlaceable { get; private set; }
    [Export] private Dictionary _savedPlaceableDictionary;
    [Export] private Sprite2D _sprite;
    
    public override void _Ready() {
        SavedPlaceable = SavedPlaceable.FromDict(_savedPlaceableDictionary);

        Position = new Vector2(
            SavedPlaceable.XLeftPosition * Game.BlockSize,
            SavedPlaceable.YBottomPosition * Game.BlockSize);
        _sprite.Texture = SavedPlaceable.Item.GetProperty<ItemPlaceable>().Texture;
        ItemPlaceable itemPlaceable = SavedPlaceable.Item.GetProperty<ItemPlaceable>();
        int offset = Game.BlockSize / 2;
        _sprite.Position = new Vector2(
            offset * (itemPlaceable.Width - 1),
            offset * (itemPlaceable.Height - 1));
    }
    
    public static ActivePlaceable Create(SavedPlaceable savedPlaceable) {
        ActivePlaceable activePlaceable = Data.PackedScenes.ActivePlaceable.Instantiate<ActivePlaceable>();
        activePlaceable._savedPlaceableDictionary = savedPlaceable.Serialize();
        return activePlaceable;
    }
}