using System;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    public event Action<Item, IntVector> BlockPlaced;
    public event Action<Item, IntVector> PlaceablePlaced;

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
                coords.ToSerialised(), _blockItem.ToDictionary());
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildActionAttempt(Array intVectorArray, Dictionary itemData) {
        IntVector coords = new(intVectorArray);
        Item blockItem = Item.FromDictionary(itemData);
        if (blockItem.HasProperty<ItemBlock>()) {
            if (_game.WorldManager.BlockManager.IsCellOccupied(coords)) return;
            BlockPlaced?.Invoke(blockItem, coords);
        }

        if (blockItem.TryGetProperty(out ItemPlaceable placeable)) {
            if (_game.WorldManager.PlaceableManager
                .AreCellsOccupied(coords, placeable.Width, placeable.Height)) {
                PlaceablePlaced?.Invoke(blockItem, coords);
            }
        }
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        if (_blockItem == stackedItems.Item) {
            _blockItem = null;
        }
    }
}