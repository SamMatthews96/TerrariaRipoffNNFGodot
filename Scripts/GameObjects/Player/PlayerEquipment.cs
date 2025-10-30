using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;
    [Export] public ItemMining Pickaxe { get; private set; }
    [Export] public ItemWeapon Weapon { get; private set; }

    public event Action<Item> ItemEquipped;

    public void InitAsLocal(Player player) {
        _player = player;
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
        _player.Game.Interface.PlayerEquipment.ClickedUnequipWeapon +=
            OnUnequipWeaponClicked;
        _player.Game.Interface.PlayerEquipment.ClickedUnequipPickaxe +=
            OnUnequipPickaxeClicked;
    }

    public override void _ExitTree() {
        _player.Game.Interface.PlayerEquipment.ClickedUnequipWeapon -=
            OnUnequipWeaponClicked;
        _player.Game.Interface.PlayerEquipment.ClickedUnequipPickaxe -=
            OnUnequipPickaxeClicked;
    }

    private void OnUnequipPickaxeClicked() {
        Pickaxe = new ItemMining();
    }

    private void OnUnequipWeaponClicked() {
        Weapon = new ItemWeapon();
    }

    private void OnEquipItemClicked(Item item) {
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment.Slot) {
            case EquipmentSlot.Mining:
                Pickaxe = item.GetProperty<ItemMining>();
                break;
            case EquipmentSlot.Weapon:

                break;
        }

        ItemEquipped?.Invoke(item);
    }
}