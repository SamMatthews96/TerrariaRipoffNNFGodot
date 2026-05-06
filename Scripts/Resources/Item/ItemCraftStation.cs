using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemCraftStation : ItemProperty {
    [Export] public StationType Type { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        return new Dictionary();
    }

    public ItemCraftStation() {
        
    }
    
    public ItemCraftStation(StationType type) {
        Type = type;
    }
    
    
}