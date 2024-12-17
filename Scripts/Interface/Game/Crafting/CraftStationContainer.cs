using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class CraftStationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;
    [Export] private PackedScene _craftingStationButtonScene;

    private Dictionary<CraftingStationType, CraftStationButton> _craftingStationButtons;

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
        CraftStationButton newButton
            = CraftStationButton.Create(craftingStation);
        newButton.CraftingStationButtonClicked += OnCraftingStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);
    }

    private void OnCraftingStationRemoved(CraftingStation craftingStation) {
        CraftStationButton button = _craftingStationButtons[craftingStation.Type];
        button.CraftingStationButtonClicked -= OnCraftingStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(craftingStation.Type);
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        CraftingStationButtonClicked?.Invoke(craftingStation);
    }
}