using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class StationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;
    [Export] private Crafting _craftingInterface;

    private Dictionary<CraftingStationType, CraftStationButton> _craftingStationButtons = new();

    public event Action<CraftingStation> CraftingStationButtonClicked;

    public override void _Ready() {
        foreach (Node node in _craftingStationButtonContainer.GetChildren()) {
            node.QueueFree();
        }

        _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned +=
            OnLocalPlayerSpawned;
        TreeExiting += () => {
            _craftingInterface.GameInterface.World.PlayerManager.LocalPlayerSpawned -=
                OnLocalPlayerSpawned;
        };
    }

    private void OnLocalPlayerSpawned(Player player) {
        OnCraftingStationAdded(CraftingStationType.Handcrafting);
        // player.Crafting.CraftingStationAdded += OnCraftingStationAdded;
        // player.Crafting.CraftingStationRemoved += OnCraftingStationRemoved;
    }

    private void OnCraftingStationAdded(CraftingStationType type) {
        CraftingStation craftingStation = Data.CraftingStations[type];
        CraftStationButton newButton
            = CraftStationButton.Create(craftingStation);
        newButton.CraftingStationButtonClicked += OnCraftingStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);

        _craftingStationButtons.Add(type, newButton);
    }

    private void OnCraftingStationRemoved(CraftingStationType type) {
        CraftStationButton button = _craftingStationButtons[type];
        button.CraftingStationButtonClicked -= OnCraftingStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(type);
    }

    private void OnCraftingStationButtonClicked(CraftingStation craftingStation) {
        CraftingStationButtonClicked?.Invoke(craftingStation);
    }
}