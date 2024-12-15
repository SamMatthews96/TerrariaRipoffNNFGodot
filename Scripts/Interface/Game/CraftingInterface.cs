using Godot;

namespace TerrariaRipoffNNF;

public partial class CraftingInterface : Control {
    [Export] private Button _selectCraftingStationMenuButton;

    [Export] private Control _availableCraftingStationContainer;
    [Export] private Control _selectedCraftingStationRecipeContainer;
    [Export] private Control _selectedRecipeContainer;
    
    
    
    public override void _Ready() {
        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
    }
   
    public override void _ExitTree() {
        Player.BeforeLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }
    
    private void OnLocalPlayerSpawned(Player player) {
        player.Crafting.CraftingStationAdded += OnCraftingStationAdded;
    }

    private void OnCraftingStationAdded(CraftingStation craftingStation) {
        
    }
}