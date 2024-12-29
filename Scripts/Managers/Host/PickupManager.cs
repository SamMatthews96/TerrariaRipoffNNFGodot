using System.Collections.Generic;
using Godot;
using Godot.Collections;
namespace TerrariaRipoffNNF;

public partial class PickupManager : Node {
    private List<SavedPickup>[,] _savedPickups;
    private List<ActivePickup>[,] _activePickups;
    
    public override void _Ready() {
        _savedPickups = new List<SavedPickup>[
            Manager.Instance.Game.Width, Manager.Instance.Game.Height];
        _activePickups = new List<ActivePickup>[
            Manager.Instance.Game.Width, Manager.Instance.Game.Height];
        Manager.Instance.Game.Host.BlockManager.BlockDestroyed += OnBlockManagerBlockDestroyed;
        Manager.Instance.Game.Host.PlayerManager.PlayerSpawned += OnPlayerManagerPlayerSpawned;
    }

    public override void _ExitTree() {
        Manager.Instance.Game.Host.BlockManager.BlockDestroyed -= OnBlockManagerBlockDestroyed;
        Manager.Instance.Game.Host.PlayerManager.PlayerSpawned -= OnPlayerManagerPlayerSpawned;
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
        player.Inventory.PickedUpItem += OnPlayerPickedUpItem;
    }

    private void OnPlayerPickedUpItem(ActivePickup activePickup) {
        DeletePickup(activePickup);
    }

    private void OnBlockManagerBlockDestroyed(SavedBlock savedBlock) {
        Vector2 position = new(savedBlock.XPosition * Game.BlockSize,
            savedBlock.YPosition * Game.BlockSize);

        CreatePickup(savedBlock.Item, position);
    }

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

        Manager.Instance.Game.BlockParent.AddChild(activePickup, true);
    }

    private void OnPickupMovedCell(ActivePickup activePickup, Dictionary positionChange) {
        IntVector previousCoords = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector coords = new(
            (int)positionChange["X"], (int)positionChange["Y"]);
        activePickup.SavedPickup.Indices = coords;

        _savedPickups[previousCoords.X, previousCoords.Y].Remove(activePickup.SavedPickup);
        _activePickups[previousCoords.X, previousCoords.Y].Remove(activePickup);
        activePickup.SavedPickup.Indices = coords;

        List<SavedPickup> savedPickupsNewPosition =
            _savedPickups[coords.X, coords.Y] ??= new List<SavedPickup>();
        List<ActivePickup> activePickupsNewPosition =
            _activePickups[coords.X, coords.Y] ??= new List<ActivePickup>();
        savedPickupsNewPosition.Add(activePickup.SavedPickup);
        activePickupsNewPosition.Add(activePickup);
    }

    private void DeletePickup(ActivePickup activePickup) {
        IntVector coords = activePickup.SavedPickup.Indices;
        _activePickups[coords.X, coords.Y].Remove(activePickup);
        _savedPickups[coords.X, coords.Y].Remove(activePickup.SavedPickup);
        activePickup.QueueFree();
    }
}