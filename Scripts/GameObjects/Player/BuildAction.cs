using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    private Item _blockItem;

    public event Action<Item, IntVector> BlockPlaced;

    public override void _Ready() {
        if (!Player.IsLocalPlayer) return;
        Manager.Instance.Game.Interface.BuildUi.BlockTypeSelected += OnBuildBlockTypeSelected;
        Player.Inventory.InventoryChanged += OnInventoryChanged;
    }

    public override void _ExitTree() {
        if (!Player.IsLocalPlayer) return;
        Manager.Instance.Game.Interface.BuildUi.BlockTypeSelected -= OnBuildBlockTypeSelected;
        Player.Inventory.InventoryChanged -= OnInventoryChanged;
    }

    private void OnBuildBlockTypeSelected(Item item) {
        _blockItem = item;
    }

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;

        float range = 8;
        if (_blockItem is not null && range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(Manager.HostId, nameof(BuildActionAttempt),
                coords.ToSerialised(), _blockItem.ToDictionary());
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildActionAttempt(Array intVectorArray, Dictionary blockTypeDict) {
        IntVector coords = new(intVectorArray);
        Item blockItem = Item.FromDictionary(blockTypeDict);

        if (Manager.Instance.Game.Host.BlockManager.IsCellOccupied(coords)) return;
        BlockPlaced?.Invoke(blockItem, coords);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryChanged(Inventory inventory) {
        if (inventory.StackedItemsList.Exists(stack =>
                stack.Item.HasProperty<ItemBlock>())) return;
        _blockItem = null;
    }
}