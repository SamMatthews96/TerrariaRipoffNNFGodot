using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class BuildAction : PlayerAction {
    private BlockType _blockType;

    public event Action<BlockType, IntVector> BlockPlaced;

    public override void _Ready() {
        if (Player.IsLocalPlayer) {
            Manager.Instance.Game.Interface.BuildUi.BlockTypeSelected += OnBuildBlockTypeSelected;
            Player.Inventory.InventoryChanged += OnInventoryChanged;
        }
    }

    private void OnBuildBlockTypeSelected(BlockType blockType) {
        _blockType = blockType;
    }

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;
        
        float buildSpeed = 1;
        float range = 8;
        if (_blockType is not null && range >= IntVector.Distance(coords, Player.Coords)) {
            RpcId(Manager.HostId, nameof(BuildActionAttempt),
                coords.ToSerialised(), _blockType.Serialize());
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void BuildActionAttempt(Array intVectorArray, Dictionary blockTypeDict) {
        IntVector coords = new(intVectorArray);
        BlockType blockType = BlockType.Deserialize(blockTypeDict);

        if (Manager.Instance.Game.Host.BlockManager.IsCellOccupied(coords)) return;
        BlockPlaced?.Invoke(blockType, coords);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) { }

    private void OnInventoryChanged(Inventory inventory) {
        if (inventory.StackedItemsList.Exists(stack => stack.ItemType == _blockType)) return;
        _blockType = null;
    }
}