using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public enum CraftingStationType {
    Handcrafting,
    Workbench,
    Furnace,
    Anvil,
    AlchemyTable,
    Loom,
    CookingPot,
}

/*  What should this class be responsible for?
    Crafting items
        knowing what recipe is selected
        knowing what ingredients are selected
        determining if an item can be crafted
        updating the inventory // or sending the events to inventory at least
        
 */

public sealed partial class Crafting : Node {
    [Export] private Player _player;
    [Export] private CraftingStation _handcrafting;

    private Dictionary<CraftingStationType, CraftingStation> _availableCraftingStations = new();
    private CraftingStation _selectedCraftingStation;


    public event Action<CraftingStation> CraftingStationAdded;

    public override void _Ready() {
        AddCraftingStation(_handcrafting);
    }

    public override void _ExitTree() { }


    private void AddCraftingStation(CraftingStation craftingStation) {
        _availableCraftingStations[craftingStation.Type] = craftingStation;
        CraftingStationAdded?.Invoke(craftingStation);
    }
}
