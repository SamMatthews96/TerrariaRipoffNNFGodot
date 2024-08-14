using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.Resources;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostPickupManager : Node {
    public static HostPickupManager Instance { get; private set; }

    [Export] private PackedScene _savedItemPickupPackedScene;

    private List<SavedItemPickup>[,] _itemPickups;

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new System.Exception("[20240814.0054.1] HostPickupManager already instantiated");
        }

        Instance = this;
    }

    public void Initialize() {
        _itemPickups = new List<SavedItemPickup>[
            GameManager.Instance.Width, GameManager.Instance.Height];
        HostBlockManager.Instance.BlockDestroyed += OnBlockManagerBlockDestroyed;
    }

    private void OnBlockManagerBlockDestroyed(SavedBlock savedBlock) {
        
        GD.Print(savedBlock);
        // Vector2 position = new(savedBlock.XPosition * GameManager.BlockSize,
        //     savedBlock.YPosition * GameManager.BlockSize);
        // GodotDictionary itemPickupData = savedBlock.BlockType.ToDictionary();
        // Rpc(nameof(CreateItemPickupOnPeer), itemPickupData, position);
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