using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public delegate void BuildActionDelegate(Item item, Vector2I coords);
    public event BuildActionDelegate HostPlaceBlockAction;
    public event BuildActionDelegate HostPlaceWallAction;
    public event BuildActionDelegate HostPlacePropAction;

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
        RpcId(1, nameof(RpcHostAttemptBuildPrimary), mouseWorldPosition,
            _blockItem.ToDictionary());
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void RpcHostAttemptBuildPrimary(
        Vector2 mouseWorldPosition, Dictionary blockItemDict
    ) {
        Vector2I coords = new(
            (int)Math.Floor(mouseWorldPosition.X / Game.BlockSize),
            (int)Math.Floor(mouseWorldPosition.Y / Game.BlockSize)
        );

        if (!Player.World.IsInBounds(coords)) return;
        float range = 8;
        if (Math.Abs(coords.X - Player.Coords.X) > range) return;
        if (Math.Abs(coords.Y - Player.Coords.Y) > range) return;

        // check that item is valid
        Item item = Item.FromDictionary(blockItemDict);
        if (item is null) {
            throw new Exception("Item is null");
        }

        if (item.HasProperty<ItemBlock>()) {
            AttemptBuildBlock(item, coords);
        } else if (item.HasProperty<ItemProp>()) {
            AttemptBuildProp(item, coords);
        }
    }

    private void AttemptBuildBlock(Item item, Vector2I coords) {
        if (Player.World.IsCellFilled(coords)) return;
        HostPlaceBlockAction?.Invoke(item, coords);
    }

    private void AttemptBuildProp(Item item, Vector2I coords) {
        ItemProp prop = item.GetProperty<ItemProp>();
        if (!Player.World.IsInBounds(coords)) return;
        Vector2I bottomRight = coords + prop.Dimensions + Vector2I.Left;
        if (!Player.World.IsInBounds(bottomRight)) return;
        for (int x = 0; x < prop.Dimensions.X; x++) {
            for (int y = 0; y < prop.Dimensions.Y; y++) {
                Vector2I cell = coords + new Vector2I(x, y);
                if (Player.World.IsCellFilled(cell)) return;
            }

            Vector2I ground = coords + new Vector2I(x, prop.Dimensions.Y);
            if (Player.World.BlockManager.Blocks[ground.X, ground.Y] is null) return;
        }

        HostPlacePropAction?.Invoke(item, coords);
    }

    public override void RightMouseAction(Vector2 mouseWorldPosition) {
        Vector2 temp = mouseWorldPosition / Game.BlockSize;
        Vector2I coords = new((int)temp.X, (int)temp.Y);
        if (_blockItem is null) return;

        if (!Player.World.IsInBounds(coords)) return;
        float range = 8;
        if (Math.Abs(coords.X - Player.Coords.X) > range) return;
        if (Math.Abs(coords.Y - Player.Coords.Y) > range) return;

        if (_blockItem.HasProperty<ItemBlock>()) {
            HostPlaceWallAction?.Invoke(_blockItem, coords);
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