using System;
using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public event Action<Item, IntVector> BuildActionAttempted;

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

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        float range = 8;
        if (_blockItem is not null && range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(SceneManager.HostId, nameof(BuildActionAttempt),
                coords, _blockItem);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildActionAttempt(IntVector coords, Item item) {
        Array<WorldObject> cellContents =
            _game.WorldObjectManager.GetCellContents(coords.X, coords.Y);
        bool doesCellHaveSpace = cellContents.Any(
            savedWorldObject => savedWorldObject is Block or PlaceableCell);
        if (doesCellHaveSpace) return;

        if (item.HasProperty<ItemBlock>() || item.HasProperty<ItemPlaceable>()) {
            BuildActionAttempted?.Invoke(item, coords);
        }
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (_blockItem == stackedItems.Item) {
            _blockItem = null;
        }
    }
}