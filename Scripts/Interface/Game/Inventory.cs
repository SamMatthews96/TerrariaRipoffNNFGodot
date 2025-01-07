using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Inventory : Control {
    [Export] private Game _gameInterface;
    [Export] private Label _capacityLabel;
    [Export] private GridContainer _inventoryItemUiContainer;
    [Export] private PackedScene _inventoryItemUiScene;

    private readonly List<InventoryItem> _inventoryItemUiList = new();

    private Player _localPlayer;

    public event Action<Control, Item> MouseEnteredItemIcon;
    public event Action MouseLeftItemIcon;
    
    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    public override void _Ready() {
        Visible = false;
        _gameInterface.GameManager.InputManager.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
        Player.BeforeLocalPlayerSpawned += OnBeforeLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        _gameInterface.GameManager.InputManager.ToggleInventoryPressed -= OnInputManagerToggleInventoryPressed;
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

    private void OnBeforeLocalPlayerLeaveScene(Player player) {
        _localPlayer.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
        _localPlayer.Inventory.ItemStackChangedSize -= OnInventoryItemStackChanged;
        _localPlayer.BeforePlayerLeaveScene -= OnBeforeLocalPlayerLeaveScene;
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        InventoryItem inventoryItemUi = _inventoryItemUiScene.Instantiate<InventoryItem>();
        _inventoryItemUiList.Add(inventoryItemUi);
        _inventoryItemUiContainer.AddChild(inventoryItemUi);
        inventoryItemUi.Update(stackedItems);
        inventoryItemUi.MouseEnteredItem += OnInventoryMouseEnteredItem;
        inventoryItemUi.MouseLeftItem += OnInventoryMouseLeftItem;
        SetCapacityLabelText();
    }
    
    private void OnInventoryMouseEnteredItem(Control node, Item item) {
        MouseEnteredItemIcon?.Invoke(node, item);
    }

    private void OnInventoryMouseLeftItem() {
        MouseLeftItemIcon?.Invoke();
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        InventoryItem inventoryItemUi = _inventoryItemUiList.Find(e =>
            e.StackedItems.Item == stackedItems.Item);
        if (inventoryItemUi != null) {
            inventoryItemUi.MouseEnteredItem -= OnInventoryMouseEnteredItem;
            inventoryItemUi.MouseLeftItem -= OnInventoryMouseLeftItem;
            inventoryItemUi.QueueFree();
            _inventoryItemUiList.Remove(inventoryItemUi);
        }
        SetCapacityLabelText();
    }

    private void OnInventoryItemStackChanged(StackedItems stackedItems) {
        InventoryItem inventoryItemUi = _inventoryItemUiList.Find(e =>
            e.StackedItems.Item == stackedItems.Item);
        inventoryItemUi.Update(stackedItems);
        SetCapacityLabelText();
    }

    private void SetCapacityLabelText() {
        _capacityLabel.Text =
            $"{_localPlayer.Inventory.UsedSpace}/{_localPlayer.Inventory.MaximumSpace}";
    }
}