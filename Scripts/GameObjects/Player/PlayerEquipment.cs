using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;
    // how items should be equipped
    // for now, just right click inventory items

    public ItemMining Pickaxe { get; private set; }
    

    public override void _Ready() {
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
    }

    private void OnEquipItemClicked(Item item) {
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment.Slot) {
            case EquipmentSlot.Mining:
                ItemMining itemMining = item.GetProperty<ItemMining>();
                Pickaxe = itemMining;
                break;
            default:
                throw new NotImplementedException();
        }
    }
}