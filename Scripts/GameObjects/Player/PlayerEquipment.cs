using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;

    public ItemMining Pickaxe { get; private set; }

    public event Action<Item> ItemEquipped;


    public override void _Ready() {
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
    }

    private void OnEquipItemClicked(Item item) {
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment.Slot) {
            case EquipmentSlot.Mining:
                Pickaxe = item.GetProperty<ItemMining>();
                break;
            default:
                throw new NotImplementedException();
        }
        // _player.Inventory.RemovedItemStack += OnEquippedItemRemoved;
        ItemEquipped?.Invoke(item);
    }

    private void OnEquippedItemRemoved(StackedItems obj) {
        //@todo if the pickaxe is removed from inventory, it should be unequipped
    }
}