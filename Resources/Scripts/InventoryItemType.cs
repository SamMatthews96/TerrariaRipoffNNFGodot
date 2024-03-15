
using System.Text.Json;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class InventoryItemType : Resource {
    [Export] public float InventorySpace { get; private set; }
    [Export] public bool IsStackable { get; private set; } = true;
    [Export] public Texture2D IconTexture { get; private set; }

    public virtual Dictionary Serialize() {
        Dictionary serialized = new();
        serialized.Add("ResourcePath",ResourcePath);
        return serialized;
    }
    
    public static InventoryItemType Deserialize(string resourcePath) {
        return ResourceLoader.Load<InventoryItemType>(resourcePath);
    }

    public static InventoryItemType Deserialize(Dictionary dictionary) {
        return Deserialize(dictionary["ResourcePath"].ToString());
    }
}