using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public event Action<Player, Item, Vector2I> BuildBlockActionAttempted;
    public event Action<Player, Item, Vector2I> BuildWallActionAttempted;

    private Item _blockItem;

    public override void _Ready() {
        Player = ActionController.Player;
        Player.World.Interface.BuildUi.BuildButtonSelected += OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
    }

    public override void _ExitTree() {
        Player.World.Interface.BuildUi.BuildButtonSelected -= OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
    }

    private void OnBuildTypeSelected(Item item) {
        _blockItem = item;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        Vector2 temp = mouseWorldPosition / Game.BlockSize;
        Vector2I coords = new((int)temp.X, (int)temp.Y);
        if (!Player.World.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is null) return;
        // get the distance between coords and Player.Coords
        float distance = (float)Math.Sqrt(
            Math.Pow(coords.X - Player.Coords.X, 2) +
            Math.Pow(coords.Y - Player.Coords.Y, 2)
        );
        if (distance > range) return;
        if (_blockItem.HasProperty<ItemPlaceable>()) {
            BuildBlockActionAttempted?.Invoke(Player, _blockItem, coords);
        }
    }


    public override void RightMouseAction(Vector2 mouseWorldPosition) {
        Vector2 temp = mouseWorldPosition / Game.BlockSize;
        Vector2I coords = new((int)temp.X, (int)temp.Y);
        if (!Player.World.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is null) return;
        float distance = (float)Math.Sqrt(
            Math.Pow(coords.X - Player.Coords.X, 2) +
            Math.Pow(coords.Y - Player.Coords.Y, 2)
        );
        if (distance > range) return;
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