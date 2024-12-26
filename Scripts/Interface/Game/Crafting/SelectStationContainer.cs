using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class SelectStationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;

    private Dictionary<CraftingStationType, SelectStationButton> _craftingStationButtons = new();

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
        SelectStationButton newButton
            = SelectStationButton.Create(craftingStation);
        newButton.CraftingStationButtonClicked += OnCraftingStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);
    }

    private void OnCraftingStationRemoved(CraftingStation craftingStation) {
        SelectStationButton button = _craftingStationButtons[craftingStation.Type];
        button.CraftingStationButtonClicked -= OnCraftingStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(craftingStation.Type);
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        CraftingStationButtonClicked?.Invoke(craftingStation);
    }
}