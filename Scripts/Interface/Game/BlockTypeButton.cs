using System;
using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class BlockTypeButton : TextureButton {
    public Item BlockItem { get; private set; }

    public event Action<Item> BuildBlockSelected;
    
    public void SetFocus() {
        Modulate = new Color(1, 1, 1);
    }

    public void SetUnfocus() {
        Modulate = new Color(1, 1, 1, 0.5f);
    }

    public static BlockTypeButton Create(Item item, bool isFocused) {
        BlockTypeButton newButton = Manager.Instance.PackedScenes
            .BlockTypeButton.Instantiate<BlockTypeButton>();
        newButton.TextureNormal = item.IconTexture;
        newButton.BlockItem = item;
        if (isFocused) {
            newButton.SetFocus();
        } else {
            newButton.SetUnfocus();
        }

        return newButton;
    }
    
    public override void _Ready() {
        ButtonDown += OnBuildBlockSelected;
    }
    
    public override void _ExitTree() {
        ButtonDown -= OnBuildBlockSelected;
    }

    private void OnBuildBlockSelected() {
        BuildBlockSelected?.Invoke(BlockItem);       
    }
    
}