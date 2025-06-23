using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;
    [Export] public ItemMining Pickaxe { get; private set; }

    public event Action<Item> ItemEquipped;

    public void InitAsLocal() {
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
    }

    private void OnEquipItemClicked(Item item) {
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment.Slot) {
            case EquipmentSlot.Mining:
                Pickaxe = item.GetProperty<ItemMining>();
                break;
            default:
                break;
        }

        ItemEquipped?.Invoke(item);
    }
}