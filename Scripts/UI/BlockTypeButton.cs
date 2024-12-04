using Godot;

namespace TerrariaRipoffNNF;

public partial class BlockTypeButton : TextureButton {
    public BlockType BlockType { get; private set; }
    
    public void SetFocus() {
        Modulate = new Color(1, 1, 1);
    }
    
    public void SetDefocus() {
        Modulate = new Color(1, 1, 1, 0.5f);
    }

    public class Builder {
        private BlockTypeButton _blockTypeButton;
        private Node _parent;

        public Builder(Node parent, PackedScene packedScene, BlockType blockType) {
            _parent = parent;
            _blockTypeButton = packedScene.Instantiate<BlockTypeButton>();
            _blockTypeButton.TextureNormal = blockType.IconTexture;
            _blockTypeButton.BlockType = blockType;
        }
        
        public Builder WithFocus(bool isFocused) {
            if (isFocused) {
                _blockTypeButton.SetFocus();
            } else {
                _blockTypeButton.SetDefocus();
            }

            return this;
        }

        public BlockTypeButton Build() {
            _parent.AddChild(_blockTypeButton);
            return _blockTypeButton;
        }
    }

    public static Builder New(Node parent, PackedScene packedScene, BlockType blockType) {
        return new Builder(parent, packedScene, blockType);
    }
}