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
    private List<InventoryItemUi> _inventoryItemUiList = new(); 

    public void Initialize(Player player) {
        _inventory = player.Inventory;
        _inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInputManagerToggleInventoryPressed() {
        Visible = !Visible;
    }

    public override void _Ready() {
        InputManager.Instance.ToggleInventoryPressed += OnInputManagerToggleInventoryPressed;
    }

    private void OnInventoryChanged() {
        GD.Print("changed");
        
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