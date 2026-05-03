using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;
    [Export] public ItemMining Pickaxe { get; private set; }
    [Export] public ItemWeapon Weapon { get; private set; }

    public event Action<Item> ItemEquipped;
    
    public override void _Ready() {
        if (!_player.IsLocalPlayer) return;
        
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
        _player.World.Interface.PlayerEquipment.ClickedUnequipWeapon +=
            OnUnequipWeaponClicked;
        _player.World.Interface.PlayerEquipment.ClickedUnequipPickaxe +=
            OnUnequipPickaxeClicked;
        TreeExiting += () => {
            _player.Inventory.EquipItemClicked -= OnEquipItemClicked;
            _player.World.Interface.PlayerEquipment.ClickedUnequipWeapon -=
                OnUnequipWeaponClicked;
            _player.World.Interface.PlayerEquipment.ClickedUnequipPickaxe -=
                OnUnequipPickaxeClicked;
        };
    }

    private void OnUnequipPickaxeClicked() {
        Pickaxe = ItemMining.Create(4, 4, 4);
    }

    private void OnUnequipWeaponClicked() {
        Weapon = null;
    }

    private void OnEquipItemClicked(Item item) {
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment.Slot) {
            case EquipmentSlot.Mining:
                Pickaxe = item.GetProperty<ItemMining>();
                break;
            case EquipmentSlot.Weapon:
                Weapon = item.GetProperty<ItemWeapon>();
                break;
        }

        ItemEquipped?.Invoke(item);
    }
}