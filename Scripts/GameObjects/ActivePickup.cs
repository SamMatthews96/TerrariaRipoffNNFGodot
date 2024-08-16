using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActivePickup : RigidBody2D {
    [Export] private Sprite2D _sprite;
    [Export] private Dictionary _savedPickupDictionary;
    [Export] private Area2D _pickupArea;
    private SavedPickup _savedPickup;

    [Signal] public delegate void TouchedPlayerEventHandler(Player player);
    
    public void Initialize(SavedPickup savedPickup) {
        HostManager.RequireHost();

        _savedPickupDictionary = savedPickup.Serialize();
    }

    public override void _Ready() {
        _savedPickup = SavedPickup.Deserialize(_savedPickupDictionary);
        Position = _savedPickup.Position;
        _sprite.Texture = _savedPickup.ItemType.IconTexture;

        if (!GameManager.Instance.IsHost) return;
        _pickupArea.BodyEntered += Test;
    }

    private void Test(Node body) {
        if (body is not Player player) {
            throw new Exception("[20240816.0053.1] ActivePickup: body is not Player");
        }
        EmitSignal(SignalName.TouchedPlayer, player);
    }

    public override void _Process(double delta) { }
}