using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActivePickup : ActiveWorldObject {
    [Export] private Sprite2D _sprite;
    [Export] private Area2D _pickupArea;

    private IntVector _previousCoords;
    public SavedPickup SavedPickup { get; private set; }
    
    private IntVector Coords => new(
        (int)Math.Round(Position.X / Game.BlockSize),
        (int)Math.Round(Position.Y / Game.BlockSize));

    public event Action<ActivePickup, Dictionary> MovedCell;

    private bool IsHost => Multiplayer.GetUniqueId() == SceneManager.HostId;
    
    public void Initialize(SavedPickup savedPickup) {
        SavedPickup = savedPickup;
        ObjectConfig = savedPickup.ToDictionary();
    }

    public override void _Ready() {
        SavedPickup ??= SavedPickup.Deserialize(ObjectConfig);
        Position = SavedPickup.Position;
        _sprite.Texture = SavedPickup.InventoryItems.Item.IconTexture;
        _previousCoords = Coords;
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