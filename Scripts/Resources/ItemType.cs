using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class ItemType : Resource {
    [Export] public float InventorySpace { get; private set; }
    [Export] public bool IsStackable { get; private set; } = true;
    [Export] public Texture2D IconTexture { get; private set; }
    [Export] public float FallWeight { get; private set; }

    public virtual Dictionary Serialize() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath", ResourcePath);
        return serialized;
    }

    public static ItemType Deserialize(Dictionary dictionary) {
        if (!dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            throw new Exception("[20240815.2158.1] ResourcePath not found in dictionary");
        }
        
        return ResourceLoader.Load<ItemType>(resourcePath.ToString());
    }
}