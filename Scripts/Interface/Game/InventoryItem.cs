using System;
using Godot;
namespace TerrariaRipoffNNF.Interface;

public partial class InventoryItem : TextureRect {
    public StackedItems StackedItems {get; private set;}
    
    [Export] private Label _countLabel;
    [Export] private TextureRect _iconTextureRect;

    public event Action<TextureRect, Item> MouseEnteredItem;
    public event Action MouseLeftItem; 

    public override void _Ready() {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseLeft;
    }

    private void OnMouseEntered() {
        MouseEnteredItem?.Invoke(this, StackedItems.Item);
    }

    private void OnMouseLeft() {
        MouseLeftItem?.Invoke();
    }

    public void Update(StackedItems stackedItems) {
        StackedItems = stackedItems;
        _countLabel.Text = StackedItems.Count.ToString();
        _iconTextureRect.Texture = StackedItems.Item.IconTexture;
    }
    
    
}