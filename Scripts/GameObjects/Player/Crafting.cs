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
