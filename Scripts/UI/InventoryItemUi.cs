using Godot;
namespace TerrariaRipoffNNF;

public partial class InventoryItemUi : TextureRect {
    private InventoryItems _inventoryItems;
    
    [Export] private Label _countLabel;
    [Export] private TextureRect _iconTextureRect;

    public void Update(InventoryItems inventoryItems) {
        _inventoryItems = inventoryItems;
        _countLabel.Text = _inventoryItems.Count.ToString();
        _iconTextureRect.Texture = _inventoryItems.ItemType.IconTexture;
    }
}