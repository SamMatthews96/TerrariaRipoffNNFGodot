using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    private readonly List<BlockTypeButton> _blockTypeButtons = new();
    [Export] private PackedScene _packedButton;
    [Export] private BoxContainer _buttonContainer;
    private Block _selectedBlock;

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
        player.ActionController.ActionChanged += OnPlayerActionChanged;
        player.Inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged(Inventory inventory) {
        _blockTypeButtons.ForEach(button => button.QueueFree());
        _blockTypeButtons.Clear();

        bool isSelectedBlockFound = false;
        inventory.StackedItemsList.ForEach(stack => {
            if (!stack.Item.TryGetProperty(out Block property)) return;
            if (property == _selectedBlock) {
                isSelectedBlockFound = true;
            }

            BlockTypeButton button =
                BlockTypeButton.New(_buttonContainer, _packedButton, stack.Item)
                    .WithFocus(property == _selectedBlock)
                    .Build();
            button.ButtonDown += () => SelectButton(button);

            _blockTypeButtons.Add(button);
        });

        if (!isSelectedBlockFound) {
            _selectedBlock = null;
        }

        if (_blockTypeButtons.Count > 0 && _selectedBlock == null) {
            SelectButton(_blockTypeButtons[0]);
        }
    }

    private void SelectButton(BlockTypeButton button) {
        _blockTypeButtons.ForEach(blockTypeButton => {
            if (blockTypeButton == button) {
                _selectedBlock = blockTypeButton.BlockItem.GetProperty<Block>();
                blockTypeButton.SetFocus();
            } else {
                blockTypeButton.SetDefocus();
            }
        });
        BlockTypeSelected?.Invoke(button.BlockItem);
    }
}