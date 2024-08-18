using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActivePickup : RigidBody2D {
    [Export] private Sprite2D _sprite;
    [Export] private Dictionary _savedPickupDictionary;
    [Export] private Area2D _pickupArea;

    private IntVector _previousCoords;
    public SavedPickup SavedPickup { get; private set; }

    private IntVector Coords => new(
        (int)Math.Round(Position.X / GameManager.BlockSize),
        (int)Math.Round(Position.Y / GameManager.BlockSize));

    public event Action<ActivePickup, Dictionary> MovedCell;

    public void Initialize(SavedPickup savedPickup) {
        HostManager.RequireHost();
        SavedPickup = savedPickup;
        _savedPickupDictionary = savedPickup.Serialize();
    }

    public override void _Ready() {
        SavedPickup ??= SavedPickup.Deserialize(_savedPickupDictionary);
        Position = SavedPickup.Position;
        _sprite.Texture = SavedPickup.InventoryItems.ItemType.IconTexture;
        _previousCoords = Coords;
    }

    public override void _PhysicsProcess(double delta) {
        if (!GameManager.Instance.IsHost) return;

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