using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Interface;

public partial class StationContainer : Control {
    [Export] private Container _craftingStationButtonContainer;
    [Export] private Crafting _craftingInterface;

    private Dictionary<StationType, CraftStationButton> _craftingStationButtons = new();

    public event Action<CraftingStation> StationButtonClicked;
    public event Action<StationType> PlayerRemovedStation;

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
        OnAddedNewStation(StationType.Handcrafting);
        player.Crafting.AddedNewStation += OnAddedNewStation;
        player.Crafting.RemovedStation += OnRemovedStation;
    }

    private void OnAddedNewStation(StationType type) {
        CraftingStation craftingStation = Data.CraftingStations[type];
        CraftStationButton newButton
            = CraftStationButton.Create(craftingStation);
        newButton.StationButtonClicked += OnStationButtonClicked;
        _craftingStationButtonContainer.AddChild(newButton);

        _craftingStationButtons.Add(type, newButton);
    }

    private void OnRemovedStation(StationType type) {
        CraftStationButton button = _craftingStationButtons[type];
        button.StationButtonClicked -= OnStationButtonClicked;
        button.QueueFree();
        _craftingStationButtons.Remove(type);
        PlayerRemovedStation?.Invoke(type);
    }

    private void OnStationButtonClicked(CraftingStation craftingStation) {
        StationButtonClicked?.Invoke(craftingStation);
    }
}