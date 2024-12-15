using System;
using Godot;

namespace TerrariaRipoffNNF;


public partial class AvailableCraftingStationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;
    [Export] private PackedScene _craftingStationButtonScene;
    // [Export] private 
    
    public event Action<CraftingStation> CraftingStationButtonClicked;
    
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
        // create a new button for the crafting station
        TextureButton newButton = _craftingStationButtonScene.Instantiate<TextureButton>();
        newButton.TextureNormal = craftingStation.Icon;
        // newButton.ButtonDown += 
        
        _craftingStationButtonContainer.AddChild(newButton);
    } 
    
    private void OnCraftingStationRemoved(CraftingStation craftingStation) {
        
    }

    private void OnCraftingStationButtonClicked() {
        
    }
}