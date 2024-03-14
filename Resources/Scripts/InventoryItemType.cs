
using Godot;

namespace TerrariaRipoffNNF.Resources.Scripts; 

public partial class InventoryItemType : Resource {
    [Export] public float InventorySpace { get; private set; }
    [Export] public bool IsStackable { get; private set; } = true;
    [Export] public Texture2D IconTexture { get; private set; }
}