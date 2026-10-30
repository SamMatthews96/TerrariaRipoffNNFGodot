using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class PlayerEquipment : Node {
    [Export] private Player _player;
    private ItemIdBimap _itemIdBimap;
    [Export] public ItemMining Pickaxe { get; private set; }
    [Export] public ItemWeapon Weapon { get; private set; }

    public event Action<Item> ItemEquipped;
    
    public override void _Ready() {
        _itemIdBimap = _player.World.ItemIdBimap;
        
        if (!_player.IsLocalPlayer) return;
        
        _player.Inventory.EquipItemClicked += OnEquipItemClicked;
        _player.World.UnequipWeaponClicked += OnUnequipWeaponClicked;
        _player.World.UnequipPickaxeClicked += OnUnequipPickaxeClicked;
        TreeExiting += () => {
            _player.Inventory.EquipItemClicked -= OnEquipItemClicked;
            _player.World.UnequipWeaponClicked -= OnUnequipWeaponClicked;
            _player.World.UnequipPickaxeClicked -= OnUnequipPickaxeClicked;
        };
    }

    private void OnUnequipPickaxeClicked() {
        Pickaxe = ItemMining.Create(4, 4, 4);
    }

    private void OnUnequipWeaponClicked() {
        Weapon = null;
    }

    private void OnEquipItemClicked(Item item) {
        ushort itemId = _itemIdBimap.GetId(item);
        this.RpcAll(nameof(RpcEquipItem), itemId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void RpcEquipItem(ushort itemId) {
        Item item = _player.World.ItemIdBimap.GetItem(itemId);
        ItemEquipment itemEquipment = item.GetProperty<ItemEquipment>();
        switch (itemEquipment) {
            case ItemMining:
                Pickaxe = item.GetProperty<ItemMining>();
                break;
            case ItemWeapon:
                Weapon = item.GetProperty<ItemWeapon>();
                break;
        }

        ItemEquipped?.Invoke(item);
    }
}