using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    private readonly Dictionary<Item, BlockTypeButton> _blockTypeButtons = new();
    [Export] private BoxContainer _buttonContainer;
    private BlockTypeButton _selectedButton;
    private Player _localPlayer;

    public event Action<Item> BlockTypeSelected;

    public override void _Ready() {
        Player.BeforeLocalPlayerSpawned += OnBeforeLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Player.BeforeLocalPlayerSpawned -= OnBeforeLocalPlayerSpawned;
    }

    private void OnPlayerActionChanged(PlayerAction.Type type) {
        if (type == PlayerAction.Type.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnBeforeLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        _localPlayer.ActionController.ActionChanged += OnPlayerActionChanged;
        _localPlayer.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        _localPlayer.BeforePlayerLeaveScene += OnBeforeLocalPlayerLeaveScene;
    }

    private void OnBeforeLocalPlayerLeaveScene() {
        _localPlayer.ActionController.ActionChanged -= OnPlayerActionChanged;
        _localPlayer.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        _localPlayer.BeforePlayerLeaveScene -= OnBeforeLocalPlayerLeaveScene;
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        if (!stackedItems.Item.TryGetProperty(out ItemBlock property)) return;
        BlockTypeButton button = BlockTypeButton.Create(stackedItems.Item, false);
        button.ButtonDown += () => SelectButton(stackedItems.Item);
        _buttonContainer.AddChild(button);
        _blockTypeButtons.Add(stackedItems.Item, button);

        if (_selectedButton == null) {
            SelectButton(stackedItems.Item);
        }
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (!_blockTypeButtons.TryGetValue(stackedItems.Item, out BlockTypeButton button)) {
            return;
        }

        if (_selectedButton == button) {
            _selectedButton = null;
        }

        button.QueueFree();
        _blockTypeButtons.Remove(stackedItems.Item);
    }

    private void SelectButton(Item item) {
        if (_selectedButton != null) {
            _selectedButton.SetUnfocus();
        }
        _selectedButton = _blockTypeButtons[item];
        _selectedButton.SetFocus();
        BlockTypeSelected?.Invoke(item);
    }
}