using Godot;

namespace TerrariaRipoffNNF.Interface;

public partial class Game : CanvasLayer {
    [Export] public TerrariaRipoffNNF.Game GameManager { get; private set; }
    [Export] public Inventory InventoryUi { get; private set; }
    [Export] public ActionBar ActionBar { get; private set; }
    [Export] public Build BuildUi { get; private set;}
    [Export] public GameMenu GameMenu { get; private set; }
    [Export] public Crafting CraftingInterface { get;private set; }
}