using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class PropStation : PropProperty {
    [Export] public CraftingStationType Type { get; private set; }

    public static PropStation Create(CraftingStationType type) {
        return new PropStation {
            Type = type
        };
    }
}