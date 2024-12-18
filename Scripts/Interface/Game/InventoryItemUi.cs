using Godot;
namespace TerrariaRipoffNNF;

public partial class InventoryItemUi : TextureRect {
    public StackedItems StackedItems {get; private set;}
    
    [Export] private Label _countLabel;
    [Export] private TextureRect _iconTextureRect;

    public void Update(StackedItems stackedItems) {
        StackedItems = stackedItems;
        _countLabel.Text = StackedItems.Count.ToString();
        _iconTextureRect.Texture = StackedItems.Item.IconTexture;
    }
}