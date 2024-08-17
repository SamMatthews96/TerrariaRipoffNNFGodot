using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class InventoryUi : Control {
    /*
     * need a reference to the inventory
     * When the local player is created, connect the inventory to the inventory ui
     */
    private Inventory _inventory;

    public void Initialize(Player player) {
        _inventory = player.Inventory;
    }

    public override void _Ready() {
        GD.Print(_inventory.MaximumSpace);
        GD.Print(_inventory.UsedSpace);
    }
}