using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Build : Container {
    [Export] private Game _gameInterface;
    [Export] private BoxContainer _blockButtonContainer;
    [Export] private BoxContainer _propButtonContainer;
    
    private readonly Dictionary<string, BlockTypeButton> _blockButtons = new();
    private readonly Dictionary<string, BlockTypeButton> _propButtons = new();

    
    private BlockTypeButton _selectedButton;

    public event Action<Item> BuildButtonSelected;

    public override void _Ready() {
        _gameInterface.World.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        foreach (Node node in _blockButtonContainer.GetChildren()) {
            node.QueueFree();
        }

        foreach (Node node in _propButtonContainer.GetChildren()) {
            node.QueueFree();
        }

        TreeExiting += () => {
            _gameInterface.World.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        };
    }
    

    private void OnPlayerActionChanged(PlayerActionType playerActionType) {
        if (playerActionType == PlayerActionType.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnLocalPlayerSpawned(Player player) {
        foreach (StackedItems stackedItems in player.Inventory.StackedItemsList) {
            OnInventoryAddedItemStack(stackedItems);    
        }
        player.ActionController.ActionChanged += OnPlayerActionChanged;
        player.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        player.TreeExiting += () => {
            player.ActionController.ActionChanged -= OnPlayerActionChanged;
            player.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
            player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        };
    }
    
    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        if (stackedItems.Item.HasProperty<ItemBlock>()) {
            AddBlockButton(stackedItems);
        }
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

    private void AddPropButton(StackedItems stackedItems) {
        BlockTypeButton button = BlockTypeButton.Create(stackedItems.Item, false);
        button.BuildBlockSelected += SelectPropButton;
        _propButtonContainer.AddChild(button);
        _propButtons.Add(stackedItems.Item.Name, button);
        if (_selectedButton == null) {
            SelectPropButton(stackedItems.Item);
        }
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (!_blockButtons.TryGetValue(stackedItems.Item.Name, out BlockTypeButton button)) {
            return;
        }

        _propButtons.Remove(stackedItems.Item.Name);
        
        
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

    private void SelectPropButton(Item item) {
        _selectedButton?.SetUnfocus();

        _selectedButton = _propButtons[item.Name];
        _selectedButton.SetFocus();
        BuildButtonSelected?.Invoke(item);
    }
}