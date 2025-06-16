using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public event Action<Player, Item, IntVector> BuildBlockActionAttempted;
    public event Action<Player, Item, IntVector> BuildWallActionAttempted;

    private Item _blockItem;
    private Game _game;

    public void InitAsLocal(Game game) {
        _game = game;
        _game.Interface.BuildUi.BuildButtonSelected += OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        TreeExiting += OnTreeExitingLocal;
    }

    public void InitAsHost(Game game) {
        _game = game;
    }

    private void OnTreeExitingLocal() {
        _game.Interface.BuildUi.BuildButtonSelected -= OnBuildTypeSelected;
        Player.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        TreeExiting -= OnTreeExitingLocal;
    }

    private void OnBuildTypeSelected(Item item) {
        _blockItem = item;
    }

    public override void LeftMouseAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is not null && range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(SceneManager.HostId, nameof(BuildBlockActionAttempt),
                coords, _blockItem);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildBlockActionAttempt(IntVector coords, Item item) {
        if (item.HasProperty<ItemPlaceable>()) {
            BuildBlockActionAttempted?.Invoke(Player, item, coords);
        }
    }

    public override void RightMouseAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is not null && range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(SceneManager.HostId, nameof(BuildWallActionAttempt),
                coords, _blockItem);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildWallActionAttempt(IntVector coords, Item item) {
        if (item.HasProperty<ItemPlaceable>()) {
            BuildWallActionAttempted?.Invoke(Player, item, coords);
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