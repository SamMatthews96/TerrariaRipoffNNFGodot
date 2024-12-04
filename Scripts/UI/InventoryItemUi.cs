using Godot;
namespace TerrariaRipoffNNF;

public partial class InventoryItemUi : TextureRect {
    private StackedItems _inventoryItems;
    
    [Export] private Label _countLabel;
    [Export] private TextureRect _iconTextureRect;

    public void Update(StackedItems stackedItems) {
        _inventoryItems = stackedItems;
        _countLabel.Text = _inventoryItems.Count.ToString();
        _iconTextureRect.Texture = _inventoryItems.ItemType.IconTexture;
    }
}