using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCraftStation : ItemProperty {
    [Export] public CraftingStationType Type { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemCraftStation() {
        
    }
    
    public ItemCraftStation(CraftingStationType type) {
        Type = type;
    }
    
    
}