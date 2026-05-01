using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.TestScenes;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public delegate void BuildActionDelegate(Item item, Vector2I coords);
    public event BuildActionDelegate HostPlacedBlock;
    public event BuildActionDelegate HostPlacedWall;
    public event BuildActionDelegate HostPlaceProp;

    private Item _blockItem;
    private ItemIdBimap _itemMap;

    public override void _Ready() {
        Player = ActionController.Player;
        _itemMap = Player.World.ItemIdBimap;
        Player.World.Interface.BuildUi.BuildButtonSelected +=
            OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack +=
            OnInventoryRemovedItemStack;
        TreeExiting += () => {
            Player.World.Interface.BuildUi.BuildButtonSelected -=
                OnBuildTypeSelected;
            Player.Inventory.RemovedItemStack -=
                OnInventoryRemovedItemStack;
        };
    }

    private void OnBuildTypeSelected(Item item) {
        _blockItem = item;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        if (_blockItem is null) return;
        ushort itemId = _itemMap.GetId(_blockItem);
        RpcId(1, nameof(RpcHostAttemptBuildPrimary),
            mouseWorldPosition, itemId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcHostAttemptBuildPrimary(
        Vector2 mousePosition, ushort itemId
    ) {
        Vector2I coords = new(
            (int)Math.Floor(mousePosition.X / Game.BlockSize),
            (int)Math.Floor(mousePosition.Y / Game.BlockSize)
        );

        int range = 8;
        if (!Player.World.IsInOrthogonalRange(coords, Player.Coords, range)) return;

        // check that item is valid
        Item item = _itemMap.GetItem(itemId);
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
        HostPlacedBlock?.Invoke(item, coords);
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

        HostPlaceProp?.Invoke(item, coords);
    }

    public override void RightMouseAction(Vector2 mousePosition) {
        if (_blockItem is null) return;
        ushort itemId = _itemMap.GetId(_blockItem);
        RpcId(1, nameof(RpcHostAttemptBuildSecondary),
            mousePosition, itemId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcHostAttemptBuildSecondary(
        Vector2 mouseWorldPosition, ushort itemId
    ) {
        Vector2I coords = new(
            (int)Math.Floor(mouseWorldPosition.X / Game.BlockSize),
            (int)Math.Floor(mouseWorldPosition.Y / Game.BlockSize)
        );

        int range = 8;
        if (!Player.World.IsInOrthogonalRange(coords, Player.Coords, range)) return;

        Item item = _itemMap.GetItem(itemId);
        if (item is null) {
            throw new Exception("Item is null");
        }

        if (item.HasProperty<ItemBlock>()) {
            AttemptBuildWall(item, coords);
        }
    }

    private void AttemptBuildWall(Item item, Vector2I coords) {
        if (Player.World.BlockManager.Walls[coords.X, coords.Y] is not null) return;
        HostPlacedWall?.Invoke(item, coords);
    }

    public override void EndLeftMouseAction(Vector2 mouseWorldPosition) { }

    public override void EndRightMouseAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (_blockItem is null) return;

        ItemIdBimap bimap = Player.World.ItemIdBimap;
        ushort itemId = bimap.GetId(stackedItems.Item);
        ushort blockId = bimap.GetId(_blockItem);
        if (itemId == blockId) {
            _blockItem = null;
        }
    }
}