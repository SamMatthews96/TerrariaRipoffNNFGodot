using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public delegate void PlaceBlockDelegate(Item item, Vector2I coords);
    public event PlaceBlockDelegate ServerPlaceBlockAction;
    public event PlaceBlockDelegate ServerPlaceWallAction;

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
        if (_blockItem is null) return;
        RpcId(1, nameof(RpcHostAttemptBuildBlock), mouseWorldPosition,
            _blockItem.ToDictionary());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostAttemptBuildBlock(
        Vector2 mouseWorldPosition, Dictionary blockItemDict
    ) {
        // check that location is valid
        Vector2I coords = (Vector2I)(mouseWorldPosition / Game.BlockSize);
        if (!Player.World.IsInBounds(coords)) return;
        float range = 8;
        float distance = (float)Math.Sqrt(
            Math.Pow(coords.X - Player.Coords.X, 2) +
            Math.Pow(coords.Y - Player.Coords.Y, 2)
        );
        if (distance > range) return;
        if (Player.World.Blocks[coords.X, coords.Y] is not null) return;

        // check that item is valid
        Item blockItem = Item.FromDictionary(blockItemDict);
        if (blockItem is null) return;
        if (!blockItem.HasProperty<ItemPlaceable>()) return;

        ServerPlaceBlockAction?.Invoke(blockItem, coords);
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
            ServerPlaceWallAction?.Invoke(_blockItem, coords);
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