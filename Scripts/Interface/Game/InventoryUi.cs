using System.Collections.Generic;
using Godot;
namespace TerrariaRipoffNNF;

public partial class InventoryUi : Control {
    [Export] private Label _capacityLabel;
    [Export] private GridContainer _inventoryItemUiContainer;
    [Export] private PackedScene _inventoryItemUiScene;

    private readonly List<InventoryItemUi> _inventoryItemUiList = new(); 
    
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
        player.Inventory.InventoryChanged += OnInventoryChanged;
    }

    private void OnInventoryChanged(Inventory inventory) {
        _capacityLabel.Text = $"{inventory.UsedSpace}/{inventory.MaximumSpace}";
        
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