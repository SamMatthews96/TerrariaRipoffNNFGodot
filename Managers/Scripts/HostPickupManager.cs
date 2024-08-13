using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.GameObjects.Scripts;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostPickupManager : Node {
    [Export] private PackedScene _savedItemPickupPackedScene;
    [Export] private HostManager _hostManager;

    private List<SavedItemPickup>[,] _itemPickups;
    
    public void Initialize() {

        _itemPickups = new List<SavedItemPickup>[
            GameManager.Instance.Width, GameManager.Instance.Height];
        // _hostManager.BlockDestroyed +=
        //     OnBlockManagerSavedBlockDestroyedOnServer;
    }

    private void OnBlockManagerSavedBlockDestroyedOnServer(SavedBlock savedBlock) {
        Vector2 position = new(savedBlock.XPosition * GameManager.BlockSize,
            savedBlock.YPosition * GameManager.BlockSize);
        GodotDictionary itemPickupData = savedBlock.BlockType.ToDictionary();
        Rpc(nameof(CreateItemPickupOnPeer), itemPickupData, position);
    }

    [Rpc(CallLocal = true)]
    private void CreateItemPickupOnPeer(GodotDictionary itemPickupData, Vector2 position) {
        InventoryItemType inventoryItemType = InventoryItemType.Deserialize(itemPickupData);

        SavedItemPickup savedItemPickup = new(inventoryItemType, position);
        AddItemPickupToLocation(savedItemPickup);
        // EmitSignal(SignalName.SavedItemPickupCreated, savedItemPickup);
    }

    private void AddItemPickupToLocation(SavedItemPickup savedItemPickup) {
        int xPosition = savedItemPickup.GridPosition.X;
        int yPosition = savedItemPickup.GridPosition.Y;
        _itemPickups[xPosition, yPosition] ??= new List<SavedItemPickup>();
        _itemPickups[xPosition, yPosition].Add(savedItemPickup);
    }

    private void RemoveItemPickupFromLocation(SavedItemPickup savedItemPickup) {
        int xPosition = savedItemPickup.GridPosition.X;
        int yPosition = savedItemPickup.GridPosition.Y;
        _itemPickups[xPosition, yPosition].Remove(savedItemPickup);
        if (_itemPickups[xPosition, yPosition].Count == 0) {
            _itemPickups[xPosition, yPosition] = null;
        }
    }
}