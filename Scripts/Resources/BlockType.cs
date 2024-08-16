using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Resources;

[GlobalClass]
public partial class BlockType : ItemType {
    [Export] public float Weight { get; private set; }
    [Export] public float TensileStrength { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    
    public new static BlockType Deserialize(Dictionary dictionary) {
        if (!dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            throw new Exception("[20240815.2158.1] ResourcePath not found in dictionary");
        }
        return ResourceLoader.Load<BlockType>(resourcePath.ToString());
    }
}