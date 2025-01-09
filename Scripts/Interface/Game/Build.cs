using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Build : Container {
    private readonly Dictionary<string, BlockTypeButton> _blockTypeButtons = new();
    [Export] private BoxContainer _buttonContainer;
    private BlockTypeButton _selectedButton;

    public event Action<Item> BlockTypeSelected;

    public override void _Ready() {
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnPlayerActionChanged(PlayerAction.Type type) {
        if (type == PlayerAction.Type.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.ActionController.ActionChanged += OnPlayerActionChanged;
        player.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        player.BeforePlayerLeaveScene += OnBeforeLocalPlayerLeaveScene;
    }

    private void OnBeforeLocalPlayerLeaveScene(Player player) {
        player.ActionController.ActionChanged -= OnPlayerActionChanged;
        player.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        player.BeforePlayerLeaveScene -= OnBeforeLocalPlayerLeaveScene;
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        if (!stackedItems.Item.HasProperty<ItemBlock>()) return;
        BlockTypeButton button = BlockTypeButton.Create(stackedItems.Item, false);
        button.BuildBlockSelected += SelectButton;
        _buttonContainer.AddChild(button);
        _blockTypeButtons.Add(stackedItems.Item.Name, button);

        if (_selectedButton == null) {
            SelectButton(stackedItems.Item);
        }
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (!_blockTypeButtons.TryGetValue(stackedItems.Item.Name, out BlockTypeButton button)) {
            return;
        }

        if (_selectedButton == button) {
            _selectedButton = null;
        }
        button.BuildBlockSelected -= SelectButton;
        button.QueueFree();
        _blockTypeButtons.Remove(stackedItems.Item.Name);
    }

    private void SelectButton(Item item) {
        if (_selectedButton != null) {
            _selectedButton.SetUnfocus();
        }
        _selectedButton = _blockTypeButtons[item.Name];
        _selectedButton.SetFocus();
        BlockTypeSelected?.Invoke(item);
    }
}