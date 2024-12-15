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

    private Dictionary<CraftingStationType, CraftingStation> _availableCraftingStations;
    private CraftingStation _selectedCraftingStation;
    
   
    public event Action<CraftingStation> CraftingStationAdded;
    
    public override void _Ready() {
        _player.MovedCell += OnPlayerMovedCell;
        // when a crafting station button is clicked, set _selectedCraftingStation
        
        CraftingStation handcraftingStation = new() {
            Type = CraftingStationType.Handcrafting
        };
        AddCraftingStation(handcraftingStation);
    }
    
    public override void _ExitTree() {
    }

    private void OnPlayerMovedCell(Dictionary _) {
        // @todo get local crafting stations
        // emit an event when craftingStations change
    }
    
    private void AddCraftingStation(CraftingStation craftingStation) {
        _availableCraftingStations[craftingStation.Type] = craftingStation;
        CraftingStationAdded?.Invoke(craftingStation);
    }
    
}

public partial class CraftingStation : Resource {
    [Export] public CraftingStationType Type;
    [Export] public Texture2D Icon;
}