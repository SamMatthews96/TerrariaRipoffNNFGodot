using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    private List<TextureButton> _blockTypeButtons = new();
    [Export] private PackedScene _packedButton;
    [Export] private BoxContainer _buttonContainer;

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
            BlockTypeButton button = BlockTypeButton.New(_packedButton, blockType);
            button.ButtonDown += () => OnBlockTypeButtonPressed(blockType);

            _blockTypeButtons.Add(button);
            _buttonContainer.AddChild(button);
        });
    }
    
    private void OnBlockTypeButtonPressed(BlockType blockType) {
        GD.Print(blockType);
    }
}