using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    private readonly List<BlockTypeButton> _blockTypeButtons = new();
    [Export] private PackedScene _packedButton;
    [Export] private BoxContainer _buttonContainer;
    private BlockType _selectedBlockType;

    public override void _Ready() {
        Player.BeforeLocalPlayerSpawned += player => {
            player.ActionController.ActionChanged += OnPlayerActionChanged;
            player.Inventory.InventoryChanged += OnInventoryChanged;
        };
    }

    private void OnPlayerActionChanged(PlayerAction.Type type) {
        if (type == PlayerAction.Type.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnInventoryChanged(Inventory inventory) {
        _blockTypeButtons.ForEach(button => button.QueueFree());
        _blockTypeButtons.Clear();
        
        inventory.StackedItemsList.ForEach(stack => {
            if (stack.ItemType is not BlockType blockType) return;
            BlockTypeButton button = 
                BlockTypeButton.New(_buttonContainer, _packedButton, blockType)
                    .WithFocus(blockType == _selectedBlockType)
                    .Build();
            button.ButtonDown += () => OnBlockTypeButtonPressed(button);

            _blockTypeButtons.Add(button);
        });
    }
    
    private void OnBlockTypeButtonPressed(BlockTypeButton button) {
        _blockTypeButtons.ForEach(blockTypeButton => {
            if (blockTypeButton == button) {
                _selectedBlockType = blockTypeButton.BlockType;
                blockTypeButton.SetFocus();
            } else {
                blockTypeButton.SetDefocus();
            }
        });
    }
}