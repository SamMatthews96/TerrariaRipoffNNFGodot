using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Build : Container {
    [Export] private Game _gameInterface;
    [Export] private BoxContainer _blockButtonContainer;
    [Export] private BoxContainer _propButtonContainer;
    
    private readonly Dictionary<string, BlockTypeButton> _blockButtons = new();
    private readonly Dictionary<ushort, BlockTypeButton> _propButtons = new();

    
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
        player.ActionState.ActionChanged += OnPlayerActionChanged;
        player.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        player.TreeExiting += () => {
            player.ActionState.ActionChanged -= OnPlayerActionChanged;
            player.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
            player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        };
    }
    
    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        if (stackedItems.Item.HasProperty<ItemBlock>()) {
            AddBlockButton(stackedItems);
        } else if (stackedItems.Item.HasProperty<ItemProp>()) {
            AddPropButton(stackedItems);
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
        ushort itemId = 
            _gameInterface.World.ItemIdBimap.GetId(stackedItems.Item);
        _propButtons.Add(itemId, button);
        if (_selectedButton == null) {
            SelectPropButton(stackedItems.Item);
        }
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (_blockButtons.TryGetValue(stackedItems.Item.Name, out BlockTypeButton button)) {
            if (_selectedButton == button) {
                _selectedButton = null;
            }

            button.BuildBlockSelected -= SelectBlockButton;
            button.QueueFree();
            _blockButtons.Remove(stackedItems.Item.Name);
        }
        ushort itemId = 
            _gameInterface.World.ItemIdBimap.GetId(stackedItems.Item);
        if (_propButtons.TryGetValue(itemId, out button)) {
            if (_selectedButton == button) {
                _selectedButton = null;
            }
            button.BuildBlockSelected -= SelectPropButton;
            button.QueueFree();
            _propButtons.Remove(itemId);
        
        }
    }

    private void SelectBlockButton(Item item) {
        _selectedButton?.SetUnfocus();

        _selectedButton = _blockButtons[item.Name];
        _selectedButton.SetFocus();
        BuildButtonSelected?.Invoke(item);
    }

    private void SelectPropButton(Item item) {
        _selectedButton?.SetUnfocus();
        ushort itemId = _gameInterface.World.ItemIdBimap.GetId(item);
        _selectedButton = _propButtons[itemId];
        _selectedButton.SetFocus();
        BuildButtonSelected?.Invoke(item);
    }
}