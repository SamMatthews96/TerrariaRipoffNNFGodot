using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Build : Container {
    private readonly Dictionary<string, BlockTypeButton> _blockButtons = new();
    private readonly Dictionary<string, BlockTypeButton> _placeableButtons = new();


    [Export] private BoxContainer _blockButtonContainer;
    [Export] private BoxContainer _placeableButtonContainer;
    private BlockTypeButton _selectedButton;

    public event Action<Item> BuildButtonSelected;

    public override void _Ready() {
        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
        foreach (Node node in _blockButtonContainer.GetChildren()) {
            node.QueueFree();
        }

        foreach (Node node in _placeableButtonContainer.GetChildren()) {
            node.QueueFree();
        }
    }

    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnPlayerActionChanged(PlayerActionType playerActionType) {
        if (playerActionType == PlayerActionType.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.ActionController.ActionChanged += OnPlayerActionChanged;
        player.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        player.PlayerDespawned += OnLocalPlayerDespawned;
    }

    private void OnLocalPlayerDespawned(Player player) {
        player.ActionController.ActionChanged -= OnPlayerActionChanged;
        player.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        player.PlayerDespawned -= OnLocalPlayerDespawned;
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        // if (stackedItems.Item.HasProperty<ItemBlock>()) {
        //     AddBlockButton(stackedItems);
        // }

        // if (stackedItems.Item.HasProperty<ItemPlaceableOld>()) {
        //     AddPlaceableButton(stackedItems);
        // }
    }

    private void AddBlockButton(StackedItems stackedItems) {
        BlockTypeButton button = BlockTypeButton.Create(stackedItems.Item, false);
        button.BuildBlockSelected += SelectBlockButton;
        _blockButtonContainer.AddChild(button);
        _blockButtons.Add(stackedItems.Item.Name, button);
        if (_selectedButton == null) {
            SelectBlockButton(stackedItems.Item);
        }
    }

    private void AddPlaceableButton(StackedItems stackedItems) {
        BlockTypeButton button = BlockTypeButton.Create(stackedItems.Item, false);
        button.BuildBlockSelected += SelectPlaceableButton;
        _placeableButtonContainer.AddChild(button);
        _placeableButtons.Add(stackedItems.Item.Name, button);
        if (_selectedButton == null) {
            SelectPlaceableButton(stackedItems.Item);
        }
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (!_blockButtons.TryGetValue(stackedItems.Item.Name, out BlockTypeButton button)) {
            return;
        }

        _placeableButtons.Remove(stackedItems.Item.Name);
        
        
        if (_selectedButton == button) {
            _selectedButton = null;
        }

        button.BuildBlockSelected -= SelectBlockButton;
        button.QueueFree();
        _blockButtons.Remove(stackedItems.Item.Name);
    }

    private void SelectBlockButton(Item item) {
        _selectedButton?.SetUnfocus();

        _selectedButton = _blockButtons[item.Name];
        _selectedButton.SetFocus();
        BuildButtonSelected?.Invoke(item);
    }

    private void SelectPlaceableButton(Item item) {
        _selectedButton?.SetUnfocus();

        _selectedButton = _placeableButtons[item.Name];
        _selectedButton.SetFocus();
        BuildButtonSelected?.Invoke(item);
    }
}