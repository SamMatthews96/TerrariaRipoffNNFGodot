using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class BlockTypeButton : TextureButton {
    public BlockType BlockType { get; private set; }
    
    public static BlockTypeButton New(PackedScene packedScene, BlockType blockType) {
        BlockTypeButton blockTypeButton = packedScene.Instantiate<BlockTypeButton>();
        blockTypeButton.TextureNormal = blockType.IconTexture;
        blockTypeButton.BlockType = blockType;
        return blockTypeButton;
    }
    
}