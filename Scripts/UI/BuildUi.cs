using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildUi : Container {
    public override void _Ready() {
        // _inventory.InventoryChanged += OnInventoryChanged;
        // Player.BeforeLocalPlayerSpawned += player => {
        //     player.ActionController.ActionChanged += OnPlayerActionChanged;
        // };
        Player.BeforeLocalPlayerSpawned += player => {
            player.ActionController.ActionChanged += OnPlayerActionChanged;
        };
    }

    private void OnPlayerActionChanged(PlayerAction.Type type) {
        if (type == PlayerAction.Type.Build) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnInventoryChanged() {
        // _blockTypeButtons.ForEach(button => button.QueueFree());
        // _blockTypeButtons.Clear();
        //
        // List<InventoryItems> blockTypes = _inventory.InventoryItemsList.FindAll(inventoryItems =>
        //     inventoryItems.ItemType is BlockType);
        // blockTypes.ForEach(blockType => {
        //     ActionBarButton button = _uiButton.Instantiate<ActionBarButton>();
        //     // button.Initialize(blockType.ItemType.IconTexture);
        //     button.ButtonDown += () => { BlockTypeSelected?.Invoke((BlockType)blockType.ItemType); };
        //
        //     _blockTypeButtons.Add(button);
        //     _blockTypesUi.AddChild(button);
        // });
    }
}