using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    private readonly List<BlockTypeButton> _blockTypeButtons = new();
    [Export] private PackedScene _packedButton;
    [Export] private BoxContainer _buttonContainer;
    private ItemBlock _selectedItemBlock;

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
            if (!stack.Item.TryGetProperty(out ItemBlock property)) return;
            if (property == _selectedItemBlock) {
                isSelectedBlockFound = true;
            }

            BlockTypeButton button =
                BlockTypeButton.Create(stack.Item, isFocused: property == _selectedItemBlock);
            button.ButtonDown += () => SelectButton(button);
            _buttonContainer.AddChild(button);
            _blockTypeButtons.Add(button);
        });

        if (!isSelectedBlockFound) {
            _selectedItemBlock = null;
        }

        if (_blockTypeButtons.Count > 0 && _selectedItemBlock == null) {
            SelectButton(_blockTypeButtons[0]);
        }
    }

    private void SelectButton(BlockTypeButton button) {
        _blockTypeButtons.ForEach(blockTypeButton => {
            if (blockTypeButton == button) {
                _selectedItemBlock = blockTypeButton.BlockItem.GetProperty<ItemBlock>();
                blockTypeButton.SetFocus();
            } else {
                blockTypeButton.SetDefocus();
            }
        });
        BlockTypeSelected?.Invoke(button.BlockItem);
    }
}