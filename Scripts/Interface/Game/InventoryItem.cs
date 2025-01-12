using System;
using Godot;
namespace TerrariaRipoffNNF.Interface;

public partial class InventoryItem : Control {
    public StackedItems StackedItems {get; private set;}
    
    [Export] private Label _countLabel;
    [Export] private TextureButton _iconTextureButton;

    public event Action<InventoryItem> MouseEnteredItem;
    public event Action MouseLeftItem;
    public event Action<StackedItems> ItemActionClicked;
    
    public override void _Ready() {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseLeft;
        _iconTextureButton.ButtonDown += OnRightClick;
    }

    private void OnRightClick() {
        ItemActionClicked?.Invoke(StackedItems);
    }

    private void OnMouseEntered() {
        MouseEnteredItem?.Invoke(this);
    }

    private void OnMouseLeft() {
        MouseLeftItem?.Invoke();
    }

    public void Update(StackedItems stackedItems) {
        StackedItems = stackedItems;
        _countLabel.Text = StackedItems.Count.ToString();
        _iconTextureButton.TextureNormal = StackedItems.Item.IconTexture;
    }
    
    
}