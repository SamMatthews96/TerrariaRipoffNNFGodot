using Godot;

namespace TerrariaRipoffNNF.scripts.Resources; 

public abstract partial class ItemResource : Resource {
    [Export] public string Name { get; set; }
    [Export] public string Description { get; set; }
    [Export] public float InventorySpace { get; set; }
    [Export] public Image Icon { get; set; }
    [Export] public bool IsStackable { get; set; }
    
}