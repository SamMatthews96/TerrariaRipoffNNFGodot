using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Pickup : WorldObject {
    [Export] private Sprite2D _sprite;
    [Export] private Area2D _pickupArea;
    public InventoryItems Items { get; private set; }

    private IntVector _previousCoords;

    private IntVector Coords => new(
        (int)Math.Round(Position.X / Game.BlockSize),
        (int)Math.Round(Position.Y / Game.BlockSize));

    public event Action<Pickup, Dictionary> MovedCell;

    private bool IsHost => Multiplayer.GetUniqueId() == SceneManager.HostId;

    public new static Pickup Create(Dictionary data) {
        Pickup newPickup = Data.PackedScenes.ActivePickup.Instantiate<Pickup>();

        Item item = Item.FromDictionary(data["item"].AsGodotDictionary());
        newPickup.Items = new InventoryItems(item);
        newPickup.XPosition = (int)Math.Round(data["xPosition"].ToString().ToFloat());
        newPickup.YPosition = (int)Math.Round(data["yPosition"].ToString().ToFloat());
        // newPickup.Disable();
        return newPickup;
    }

    public override void _Ready() {
        Position = new Vector2(
            XPosition * Game.BlockSize,
            YPosition * Game.BlockSize);
        _previousCoords = Coords;
        ItemBlock itemBlock = Items.Item.GetProperty<ItemBlock>();
        _sprite.Texture = itemBlock.Texture;
    }

    public override void _PhysicsProcess(double delta) {
        if (!IsHost) return;

        if (_previousCoords != Coords) {
            Dictionary positionChange = new() {
                { "X", Coords.X },
                { "Y", Coords.Y },
                { "PreviousX", _previousCoords.X },
                { "PreviousY", _previousCoords.Y }
            };

            MovedCell?.Invoke(this, positionChange);
        }

        _previousCoords = Coords;
    }
}