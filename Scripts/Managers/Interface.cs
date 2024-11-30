using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class Interface : CanvasLayer {
    public static Interface Instance { get; private set; }

    [Export] public InventoryUi InventoryUi { get; private set; }
    [Export] public ActionBar ActionBar { get; private set; }
    [Export] public BuildUi BuildUi { get; private set; }
    
    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240817.0244.1] UiManager already instantiated");
        }

        Instance = this;
    }
}