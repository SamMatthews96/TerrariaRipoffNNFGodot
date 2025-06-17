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

        Player.LocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    public override void _ExitTree() {
        Player.LocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    private void OnLocalPlayerSpawned(Player player) {
        player.Crafting.CraftingStationAdded += OnCraftingStationAdded;
        player.Crafting.CraftingStationRemoved += OnCraftingStationRemoved;
    }

    private void OnCraftingStationAdded(CraftingStationType type) {
        CraftingStation craftingStation = Data.CraftingStations[type];
        SelectStationButton newButton
            = SelectStationButton.Create(craftingStation);
        newButton.CraftingStationButtonClicked += OnCraftingStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);

        _craftingStationButtons.Add(type, newButton);
    }

    private void OnCraftingStationRemoved(CraftingStationType type) {
        SelectStationButton button = _craftingStationButtons[type];
        button.CraftingStationButtonClicked -= OnCraftingStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(type);
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        CraftingStationButtonClicked?.Invoke(craftingStation);
    }
}