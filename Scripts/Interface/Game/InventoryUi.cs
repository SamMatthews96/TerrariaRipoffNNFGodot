using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class InventoryUi : Control {
    [Export] private Label _capacityLabel;
    [Export] private GridContainer _inventoryItemUiContainer;
    [Export] private PackedScene _inventoryItemUiScene;

    private readonly List<InventoryItemUi> _inventoryItemUiList = new();

    private Player _localPlayer;

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    public override void _Ready() {
        Visible = false;
        Manager.Instance.Game.InputManager.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
        Player.BeforeLocalPlayerSpawned += OnBeforeLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Manager.Instance.Game.InputManager.ToggleInventoryPressed -= OnInputManagerToggleInventoryPressed;
        Player.BeforeLocalPlayerSpawned -= OnBeforeLocalPlayerSpawned;
    }

    private void OnBeforeLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        SetCapacityLabelText();
        _localPlayer.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        _localPlayer.Inventory.ItemStackChangedSize += OnInventoryItemStackChanged;
        _localPlayer.BeforePlayerLeaveScene += OnBeforeLocalPlayerLeaveScene;
    }

    private void OnBeforeLocalPlayerLeaveScene() {
        _localPlayer.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        _localPlayer.Inventory.ItemStackChangedSize -= OnInventoryItemStackChanged;
        _localPlayer.BeforePlayerLeaveScene -= OnBeforeLocalPlayerLeaveScene;
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        InventoryItemUi inventoryItemUi = _inventoryItemUiScene.Instantiate<InventoryItemUi>();
        _inventoryItemUiList.Add(inventoryItemUi);
        _inventoryItemUiContainer.AddChild(inventoryItemUi);
        inventoryItemUi.Update(stackedItems);
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        InventoryItemUi inventoryItemUi = _inventoryItemUiList.Find(e =>
            e.StackedItems.Item == stackedItems.Item);
        if (inventoryItemUi != null) {
            inventoryItemUi.QueueFree();
            _inventoryItemUiList.Remove(inventoryItemUi);
        }
    }

    private void OnInventoryItemStackChanged(StackedItems stackedItems) {
        InventoryItemUi inventoryItemUi = _inventoryItemUiList.Find(e =>
            e.StackedItems.Item == stackedItems.Item);
        inventoryItemUi.Update(stackedItems);
    }

    private void SetCapacityLabelText() {
        _capacityLabel.Text =
            $"{_localPlayer.Inventory.UsedSpace}/{_localPlayer.Inventory.MaximumSpace}";
    }

    private void OnInventoryChanged(Inventory inventory) {
        SetCapacityLabelText();
        _inventoryItemUiList.ForEach(e => e.QueueFree());
        _inventoryItemUiList.Clear();
        inventory.StackedItemsList.ForEach(e => {
            InventoryItemUi inventoryItemUi = _inventoryItemUiScene.Instantiate<InventoryItemUi>();
            _inventoryItemUiList.Add(inventoryItemUi);
            _inventoryItemUiContainer.AddChild(inventoryItemUi);
            inventoryItemUi.Update(e);
        });
    }
}