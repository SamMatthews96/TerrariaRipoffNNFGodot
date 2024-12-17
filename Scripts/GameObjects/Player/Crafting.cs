using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public enum CraftingStationType {
    Handcrafting,
    Workbench,
    Furnace,
    Anvil,
    AlchemyTable
}

public sealed partial class Crafting : Node {
    [Export] private Player _player;

    private Dictionary<CraftingStationType, CraftingStation> _availableCraftingStations = new();
    private CraftingStation _selectedCraftingStation;


    public event Action<CraftingStation> CraftingStationAdded;

    public override void _Ready() {
        CraftingStation handcraftingStation = new() {
            Type = CraftingStationType.Handcrafting
            //@todo add icon
        };
        AddCraftingStation(handcraftingStation);
    }

    public override void _ExitTree() { }


    private void AddCraftingStation(CraftingStation craftingStation) {
        _availableCraftingStations[craftingStation.Type] = craftingStation;
        CraftingStationAdded?.Invoke(craftingStation);
    }
}

//@todo move this to a separate file
public partial class CraftingStation : Resource {
    [Export] public CraftingStationType Type;
    [Export] public Texture2D Icon;
}