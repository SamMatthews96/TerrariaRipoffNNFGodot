using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class PlayerEquipment : Container {
    private Player _localPlayer;
    [Export] private Game _gameInterface;
    
    [Export] private TextureButton _weaponIcon;
    [Export] private TextureButton _pickaxeIcon;
    
    public event Action ClickedUnequipWeapon;
    public event Action ClickedUnequipPickaxe;

    public override void _Ready() {
        Visible = false;
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;

        _gameInterface.GameManager.InputManager.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
        _gameInterface.GameManager.InputManager.EscapePressed += OnEscapePressed;
        _weaponIcon.Pressed += OnWeaponIconPressed;
        _pickaxeIcon.Pressed += OnMiningIconPressed;
    }

    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        _gameInterface.GameManager.InputManager.ToggleInventoryPressed -= OnInputManagerToggleInventoryPressed;
        _gameInterface.GameManager.InputManager.EscapePressed -= OnEscapePressed;
        _weaponIcon.Pressed -= OnWeaponIconPressed;
        _pickaxeIcon.Pressed -= OnMiningIconPressed;
    }

    private void OnMiningIconPressed() {
        ClickedUnequipPickaxe?.Invoke();
    }

    private void OnWeaponIconPressed() {
        ClickedUnequipWeapon?.Invoke();
    }
    

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    private void OnEscapePressed() {
        if (Visible) {
            Visible = false;
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        _localPlayer.PlayerEquipment.ItemEquipped += OnItemEquipped;
    }

    private void OnItemEquipped(Item item) {
        ItemEquipment equipment = item.GetProperty<ItemEquipment>();
        switch (equipment.Slot) {
            case EquipmentSlot.Mining:
                _pickaxeIcon.TextureNormal = item.IconTexture;
                break;
            case EquipmentSlot.Weapon:
                _weaponIcon.TextureNormal = item.IconTexture;
                break;
        }
    }
}