using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class BuildUi : Container {
    [Export] private Container _buildDrawModeUi;
    [Export] private Container _blockTypesUi;
    [Export] private PackedScene _uiButton;

    private Inventory _inventory;
    private List<ActionBarButton> _drawModeButtons = new();
    private List<ActionBarButton> _blockTypeButtons = new();

    public void Initialize(Inventory inventory) {
        _inventory = inventory;
        _inventory.InventoryChanged += OnInventoryChanged;

        Player.LocalPlayer.BuildStateEntered += OnBuildStateEntered;
        Player.LocalPlayer.BuildStateLeft += OnBuildStateLeft;
    }

    public override void _Ready() {
        Hide();
    }

    private void OnInventoryChanged() {
        _blockTypeButtons.ForEach(button => button.QueueFree());
        _blockTypeButtons.Clear();

        List<InventoryItems> blockTypes = _inventory.InventoryItemsList.FindAll(inventoryItems =>
            inventoryItems.ItemType is BlockType);
        blockTypes.ForEach(blockType => {
            ActionBarButton button = _uiButton.Instantiate<ActionBarButton>();
            button.Initialize(blockType.ItemType.IconTexture);
            _blockTypeButtons.Add(button);
            _blockTypesUi.AddChild(button);
        });
    }

    private void OnBuildStateEntered() {
        Show();
    }

    private void OnBuildStateLeft() {
        Hide();
    }
}