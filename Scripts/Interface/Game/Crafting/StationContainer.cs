using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class StationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;
    [Export] private PackedScene _craftingStationButtonScene;

    private Dictionary<CraftingStationType, StationButton> _craftingStationButtons;

    public event Action<CraftingStation> CraftingStationButtonClicked;

    public override void _Ready() {
        foreach (Node node in _craftingStationButtonContainer.GetChildren()) {
            node.QueueFree();
        }
        Player.BeforeLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Player.BeforeLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.Crafting.CraftingStationAdded += OnCraftingStationAdded;
    }

    private void OnCraftingStationAdded(CraftingStation craftingStation) {
        StationButton newButton
            = StationButton.Create(craftingStation);
        newButton.CraftingStationButtonClicked += OnCraftingStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);
    }

    private void OnCraftingStationRemoved(CraftingStation craftingStation) {
        StationButton button = _craftingStationButtons[craftingStation.Type];
        button.CraftingStationButtonClicked -= OnCraftingStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(craftingStation.Type);
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        CraftingStationButtonClicked?.Invoke(craftingStation);
    }
}