using Godot;

namespace TerrariaRipoffNNF;

public partial class Interface : CanvasLayer {
    [Export] public InventoryUi InventoryUi { get; private set; }
    [Export] public ActionBar ActionBar { get; private set; }
    // [Export] public BuildUi BuildUi { get; private set; }
    
}