using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public event Action<Player, Item, IntVector> BuildBlockActionAttempted;
    public event Action<Player, Item, IntVector> BuildWallActionAttempted;

    private Item _blockItem;

    public override void _Ready() {
        Player = ActionController.Player;
        Game = ActionController.Game;
        Game.Interface.BuildUi.BuildButtonSelected += OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
    }

    public override void _ExitTree() {
        Game.Interface.BuildUi.BuildButtonSelected -= OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
    }

    private void OnBuildTypeSelected(Item item) {
        _blockItem = item;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!Game.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is null) return;
        if (IntVector.Distance(coords, Player.Coords) > range) return;
        if (_blockItem.HasProperty<ItemPlaceable>()) {
            BuildBlockActionAttempted?.Invoke(Player, _blockItem, coords);
        }
    }



    public override void RightMouseAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!Game.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is null) return;
        if (range > IntVector.Distance(coords, Player.Coords)) return;
        if (_blockItem.HasProperty<ItemPlaceable>()) {
            BuildWallActionAttempted?.Invoke(Player, _blockItem, coords);
        }
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }

    public override void EndRightMouseAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (_blockItem == stackedItems.Item) {
            _blockItem = null;
        }
    }
}