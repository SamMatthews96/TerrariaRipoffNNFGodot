using Godot;

namespace TerrariaRipoffNNF;

public partial class SelectedCraftingStationRecipeContainer : Node {
    [Export] public CraftingInterface CraftingInterface { get; private set; }
    

    public override void _Ready() {
        CraftingInterface.AvailableCraftingStationContainer.CraftingStationButtonClicked +=
            OnCraftingStationButtonClicked;
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        GD.Print("player wants to see " + craftingStation.Type + " recipes");
    }
}