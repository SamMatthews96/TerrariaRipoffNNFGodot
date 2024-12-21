using System.Collections.Generic;
using System.Globalization;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class ItemTooltip : Control {
    [Export] private Inventory _inventory;
    [Export] private Control _propertyContainer;

    [Export] private Label _nameLabel;
    [Export] private Label _inventorySpaceLabel;

    private readonly List<Control> _itemPropertyLabelGroups = new();

    public override void _Ready() {
        Hide();
        _inventory.MouseEnteredItemIcon += OnInterfaceMouseEnteredItemMouseEntered;
        _inventory.MouseLeftItemIcon += OnInterfaceMouseEnteredItemMouseExited;
    }

    private void OnInterfaceMouseEnteredItemMouseEntered(Control node, Item item) {
        _nameLabel.Text = item.Name;
        _inventorySpaceLabel.Text =
            $"Space: {item.InventorySpace.ToString(CultureInfo.InvariantCulture)}";
        foreach ((string propertyName, Dictionary itemProperty) in item.GetTooltipAttributes()) {
            ItemPropertyTooltipGroup newGroup =
                ItemPropertyTooltipGroup.Create(propertyName, itemProperty);
            _propertyContainer.AddChild(newGroup);
            _itemPropertyLabelGroups.Add(newGroup);
        }

        GlobalPosition = node.GlobalPosition + node.Size;
        Show();
    }

    private void OnInterfaceMouseEnteredItemMouseExited() {
        foreach (Control label in _itemPropertyLabelGroups) {
            label.QueueFree();
        }

        _itemPropertyLabelGroups.Clear();
        Hide();
    }
}