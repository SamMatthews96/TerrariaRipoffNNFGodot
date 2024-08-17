using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF.Scripts.UI;

public partial class InventoryUi : Control {
    [Export] private Label _capacityLabel;
    [Export] private GridContainer _inventoryItemUiContainer;
    [Export] private PackedScene _inventoryItemUiScene;

    private Inventory _inventory;
    private readonly List<InventoryItemUi> _inventoryItemUiList = new(); 

    public void Initialize(Inventory inventory) {
        _inventory = inventory;
        _inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    public override void _Ready() {
        Visible = false;
        InputManager.Instance.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
    }

    private void OnInventoryChanged() {
        _capacityLabel.Text = $"{_inventory.UsedSpace}/{_inventory.MaximumSpace}";
        
        _inventoryItemUiList.ForEach(e => e.QueueFree());
        _inventoryItemUiList.Clear();
        _inventory.InventoryItemsList.ForEach(e => {
            InventoryItemUi inventoryItemUi = _inventoryItemUiScene.Instantiate<InventoryItemUi>();
            _inventoryItemUiList.Add(inventoryItemUi);
            _inventoryItemUiContainer.AddChild(inventoryItemUi);
            inventoryItemUi.Update(e);
        });
        
    }
}