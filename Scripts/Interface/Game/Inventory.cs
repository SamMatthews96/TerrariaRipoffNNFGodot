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

    public event Action<InventoryItem> MouseEnteredItemIcon;
    public event Action MouseLeftItemIcon;
    public event Action<StackedItems> ItemActionClicked;

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    private void OnEscapePressed() {
        if (Visible) {
            Visible = false;
        }
    }

    public override void _Ready() {
        Visible = false;
        _gameInterface.World.InputManager.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
        _gameInterface.World.InputManager.EscapePressed += OnEscapePressed;
        _gameInterface.World.PlayerManager.LocalPlayerSpawned += OnLocalPlayerSpawned;
        TreeExiting += () => {
            _gameInterface.World.InputManager.ToggleInventoryPressed -= OnInputManagerToggleInventoryPressed;
            _gameInterface.World.InputManager.EscapePressed -= OnEscapePressed;
            _gameInterface.World.PlayerManager.LocalPlayerSpawned -= OnLocalPlayerSpawned;
        };
    }

    private void OnLocalPlayerSpawned(Player player) {
        _localPlayer = player;
        List<StackedItems> stackedItemsList = 
            _localPlayer.Inventory.StackedItemsList;
        foreach (StackedItems stackedItems in stackedItemsList) {
            OnInventoryAddedItemStack(stackedItems);
        }
        SetCapacityLabelText();

        _localPlayer.Inventory.AddedItemStack += OnInventoryAddedItemStack;
        _localPlayer.Inventory.RemovedItemStack += OnInventoryRemovedItemStack;
        _localPlayer.Inventory.ItemStackChangedSize += OnInventoryItemStackChanged;
        _localPlayer.TreeExiting += () => {
            _localPlayer.Inventory.AddedItemStack -= OnInventoryAddedItemStack;
            _localPlayer.Inventory.RemovedItemStack -= OnInventoryRemovedItemStack;
            _localPlayer.Inventory.ItemStackChangedSize -= OnInventoryItemStackChanged;
        };
    }

    private void OnInventoryAddedItemStack(StackedItems stackedItems) {
        InventoryItem inventoryItemUi = _inventoryItemUiScene.Instantiate<InventoryItem>();
        inventoryItemUi.ItemActionClicked += OnItemActionClicked;
        _inventoryItemUiList.Add(inventoryItemUi);
        _inventoryItemUiContainer.AddChild(inventoryItemUi);
        inventoryItemUi.Update(stackedItems);
        inventoryItemUi.MouseEnteredItem += OnInventoryMouseEnteredItem;
        inventoryItemUi.MouseLeftItem += OnInventoryMouseLeftItem;
        SetCapacityLabelText();
    }

    private void OnItemActionClicked(StackedItems stackedItems) {
        ItemActionClicked?.Invoke(stackedItems);
    }

    private void OnInventoryMouseEnteredItem(InventoryItem item) {
        MouseEnteredItemIcon?.Invoke(item);
    }

    private void OnInventoryMouseLeftItem() {
        MouseLeftItemIcon?.Invoke();
    }

    private void OnInventoryRemovedItemStack(StackedItems stackedItems) {
        InventoryItem inventoryItemUi = _inventoryItemUiList.Find(e =>
            Item.AreEqual(e.StackedItems.Item, stackedItems.Item));
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
            $"{Math.Round(_localPlayer.Inventory.UsedSpace, 2)}/{_localPlayer.Inventory.MaximumSpace}";
    }
}