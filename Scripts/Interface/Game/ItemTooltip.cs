using System.Globalization;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class ItemTooltip : Control {
    [Export] private Inventory _inventory;

    [Export] private Label _nameLabel;
    [Export] private Label _inventorySpaceLabel;

    public override void _Ready() {
        Hide();
        _inventory.MouseEnteredItemIcon += OnInterfaceMouseEnteredItemMouseEntered;
        _inventory.MouseLeftItemIcon += OnInterfaceMouseEnteredItemMouseExited;
    }


    private void OnInterfaceMouseEnteredItemMouseEntered(Control node, Item item) {
        _nameLabel.Text = item.Name;
        _inventorySpaceLabel.Text =
            $"Space: {item.InventorySpace.ToString(CultureInfo.InvariantCulture)}";

        Show();
    }

    private void OnInterfaceMouseEnteredItemMouseExited() {
        Hide();
    }
}