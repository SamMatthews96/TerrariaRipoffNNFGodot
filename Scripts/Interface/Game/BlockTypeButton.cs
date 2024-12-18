using Godot;

namespace TerrariaRipoffNNF;

public partial class BlockTypeButton : TextureButton {
    public Item BlockItem { get; private set; }

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
}