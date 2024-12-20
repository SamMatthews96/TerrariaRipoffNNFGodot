using System.Globalization;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class ItemTooltip : Control {
    [Export] private Inventory _inventory;
    [Export] private Control _labelContainer;

    [Export] private Label _nameLabel;
    [Export] private Label _inventorySpaceLabel;

    [Export] private PackedScene _packedLabel;

    public override void _Ready() {
        Hide();
        _inventory.MouseEnteredItemIcon += OnInterfaceMouseEnteredItemMouseEntered;
        _inventory.MouseLeftItemIcon += OnInterfaceMouseEnteredItemMouseExited;
    }

    private void OnInterfaceMouseEnteredItemMouseEntered(Control node, Item item) {
        _nameLabel.Text = item.Name;
        _inventorySpaceLabel.Text =
            $"Space: {item.InventorySpace.ToString(CultureInfo.InvariantCulture)}";
        // @todo create labels to show stats
        item.GetTooltipAttributes();
        Show();
    }

    private void OnInterfaceMouseEnteredItemMouseExited() {
        Hide();
    }
}