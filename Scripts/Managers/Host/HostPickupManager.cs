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
    }

    private void OnBlockManagerBlockDestroyed(SavedBlock savedBlock) {
        Vector2 position = new(savedBlock.XPosition * GameManager.BlockSize,
            savedBlock.YPosition * GameManager.BlockSize);

        AddPickup(savedBlock.BlockType, position);
    }

    private void AddPickup(ItemType itemType, Vector2 position) {
        IntVector coords = new(position / GameManager.BlockSize);

        SavedPickup savedPickup = new(itemType, position);
        _savedPickups[coords.X, coords.Y] ??= new List<SavedPickup>();
        _savedPickups[coords.X, coords.Y].Add(savedPickup);

        ActivePickup activePickup = _pickupPackedScene.Instantiate<ActivePickup>();
        activePickup.Initialize(savedPickup);
        _activePickups[coords.X, coords.Y] ??= new List<ActivePickup>();
        _activePickups[coords.X, coords.Y].Add(activePickup);

        GameManager.Instance.BlockParent.AddChild(activePickup, true);
    }

    // [Rpc(CallLocal = true)]
    // private void CreateItemPickupOnPeer(GodotDictionary itemPickupData, Vector2 position) {
    //     InventoryItemType inventoryItemType = InventoryItemType.Deserialize(itemPickupData);
    //
    //     SavedItemPickup savedItemPickup = new(inventoryItemType, position);
    //     AddItemPickupToLocation(savedItemPickup);
    //     // EmitSignal(SignalName.SavedItemPickupCreated, savedItemPickup);
    // }

    // private void AddItemPickupToLocation(SavedItemPickup savedItemPickup) {
    //     int xPosition = savedItemPickup.GridPosition.X;
    //     int yPosition = savedItemPickup.GridPosition.Y;
    //     _itemPickups[xPosition, yPosition] ??= new List<SavedItemPickup>();
    //     _itemPickups[xPosition, yPosition].Add(savedItemPickup);
    // }
    //
    // private void RemoveItemPickupFromLocation(SavedItemPickup savedItemPickup) {
    //     int xPosition = savedItemPickup.GridPosition.X;
    //     int yPosition = savedItemPickup.GridPosition.Y;
    //     _itemPickups[xPosition, yPosition].Remove(savedItemPickup);
    //     if (_itemPickups[xPosition, yPosition].Count == 0) {
    //         _itemPickups[xPosition, yPosition] = null;
    //     }
    // }
}