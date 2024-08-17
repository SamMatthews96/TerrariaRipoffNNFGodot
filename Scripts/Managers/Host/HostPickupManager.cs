using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostPickupManager : Node {
    public static HostPickupManager Instance { get; private set; }

    [Export] private PackedScene _pickupPackedScene;

    private List<SavedPickup>[,] _savedPickups;
    private List<ActivePickup>[,] _activePickups;

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new System.Exception("[20240814.0054.1] HostPickupManager already instantiated");
        }

        Instance = this;
    }

    public void Initialize() {
        _savedPickups = new List<SavedPickup>[
            GameManager.Instance.Width, GameManager.Instance.Height];
        _activePickups = new List<ActivePickup>[
            GameManager.Instance.Width, GameManager.Instance.Height];
        HostBlockManager.Instance.BlockDestroyed += OnBlockManagerBlockDestroyed;
        HostPlayerManager.Instance.PlayerSpawned += OnPlayerManagerPlayerSpawned;
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
        player.PickedUpItem += OnPlayerPickedUpItem;
    }

    private void OnPlayerPickedUpItem(ActivePickup activePickup) {
        DeletePickup(activePickup);
    }

    private void OnBlockManagerBlockDestroyed(SavedBlock savedBlock) {
        Vector2 position = new(savedBlock.XPosition * GameManager.BlockSize,
            savedBlock.YPosition * GameManager.BlockSize);

        CreatePickup(savedBlock.BlockType, position);
    }

    private void CreatePickup(ItemType itemType, Vector2 position) {
        IntVector coords = new(position / GameManager.BlockSize);

        SavedPickup savedPickup = new(itemType, position);
        _savedPickups[coords.X, coords.Y] ??= new List<SavedPickup>();
        _savedPickups[coords.X, coords.Y].Add(savedPickup);

        ActivePickup activePickup = _pickupPackedScene.Instantiate<ActivePickup>();
        activePickup.Initialize(savedPickup);
        _activePickups[coords.X, coords.Y] ??= new List<ActivePickup>();
        _activePickups[coords.X, coords.Y].Add(activePickup);
        activePickup.MovedCell += OnPickupMovedCell;

        GameManager.Instance.BlockParent.AddChild(activePickup, true);
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