using System;
using Godot;
using TerrariaRipoffNNF.Scripts.UI;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class UiManager : CanvasLayer {
    public static UiManager Instance { get; private set; }

    [Export] public InventoryUi InventoryUi { get; private set; }
    [Export] public ActionBar ActionBar { get; private set; }
    
    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240817.244.1] UiManager already instantiated");
        }

        Instance = this;
    }
}