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
        _player.Game.World.Interface.PlayerEquipment.ClickedUnequipWeapon +=
            OnUnequipWeaponClicked;
        _player.Game.World.Interface.PlayerEquipment.ClickedUnequipPickaxe +=
            OnUnequipPickaxeClicked;
        TreeExiting += OnTreeExiting;
    }

    private void OnTreeExiting() {
        TreeExiting -= OnTreeExiting;
        _player.Game.World.Interface.PlayerEquipment.ClickedUnequipWeapon -=
            OnUnequipWeaponClicked;
        _player.Game.World.Interface.PlayerEquipment.ClickedUnequipPickaxe -=
            OnUnequipPickaxeClicked;
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