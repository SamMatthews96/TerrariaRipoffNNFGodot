using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class PlayerEquipment : Container {
    private Player _localPlayer;
    [Export] private TextureRect _pickaxeIcon;
    [Export] private Game _gameInterface; 
    public override void _Ready() {
        Visible = false;
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
        
        _gameInterface.GameManager.InputManager.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
        
    }
    
    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        _gameInterface.GameManager.InputManager.ToggleInventoryPressed -= OnInputManagerToggleInventoryPressed;
    }

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        _localPlayer.PlayerEquipment.ItemEquipped += OnItemEquipped;
    }
    
    private void OnItemEquipped(Item item) {
        ItemEquipment equipment = item.GetProperty<ItemEquipment>();
        switch (equipment.Slot) {
            case EquipmentSlot.Mining:
                _pickaxeIcon.Texture = item.IconTexture;
                break;
        }
        
    }
}