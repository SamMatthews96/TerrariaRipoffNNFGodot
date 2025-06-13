using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ItemPlaceable : ItemProperty {
    [Export] public SavedObject SavedObject { get; private set; }
    
    public override Dictionary GetTooltipAttributes() {
        throw new System.NotImplementedException();
    }
    
    
}