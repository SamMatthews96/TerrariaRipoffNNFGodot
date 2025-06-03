using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PickupManager : Node {
    private Game _game;
    private List<SavedPickup>[,] _savedPickups;
    private List<ActivePickup>[,] _activePickups;

    private void CreatePickup(Item item, Vector2 position) {
        IntVector coords = new(position / Game.BlockSize);

        SavedPickup savedPickup = new(item, position);
        _savedPickups[coords.X, coords.Y] ??= new List<SavedPickup>();
        _savedPickups[coords.X, coords.Y].Add(savedPickup);

        ActivePickup activePickup = Data.PackedScenes.ActivePickup.Instantiate<ActivePickup>();
        activePickup.Initialize(savedPickup);
        _activePickups[coords.X, coords.Y] ??= new List<ActivePickup>();
        _activePickups[coords.X, coords.Y].Add(activePickup);
        activePickup.MovedCell += OnPickupMovedCell;

        _game.BlockParent.AddChild(activePickup, true);
    }

    private void OnPickupMovedCell(ActivePickup activePickup, Dictionary positionChange) {
        IntVector previousCoords = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector coords = new(
            (int)positionChange["X"], (int)positionChange["Y"]);
        // activePickup.SavedPickup.Indices = coords;

        _savedPickups[previousCoords.X, previousCoords.Y].Remove(activePickup.SavedPickup);
        _activePickups[previousCoords.X, previousCoords.Y].Remove(activePickup);
        // activePickup.SavedPickup.Indices = coords;

        List<SavedPickup> savedPickupsNewPosition =
            _savedPickups[coords.X, coords.Y] ??= new List<SavedPickup>();
        List<ActivePickup> activePickupsNewPosition =
            _activePickups[coords.X, coords.Y] ??= new List<ActivePickup>();
        savedPickupsNewPosition.Add(activePickup.SavedPickup);
        activePickupsNewPosition.Add(activePickup);
    }
}