using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.GameObjects.Scripts;
using GodotDictionary = Godot.Collections.Dictionary;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class ItemPickupManager : Node {
    private List<SavedItemPickup>[,] _itemPickups;
    private int _width;
    private int _height;

    [Signal]
    public delegate void SavedItemPickupCreatedEventHandler(SavedItemPickup savedItemPickup);

    [Signal]
    public delegate void SavedItemPickupDeletedEventHandler(SavedItemPickup savedItemPickup);

    public static ItemPickupManager Instance { get; private set; }
    
    public override void _EnterTree() {
        Instance = this;
    }

    public void Initialize(GodotDictionary worldDictionary) {
        _width = (int)worldDictionary["Width"];
        _height = (int)worldDictionary["Height"];
        _itemPickups = new List<SavedItemPickup>[_width, _height];
        BlockManager.Instance.SavedBlockDestroyedOnServer += OnBlockManagerSavedBlockDestroyedOnServer;
    }

    private void OnBlockManagerSavedBlockDestroyedOnServer(SavedBlock savedBlock) {
        Vector2 position = new(savedBlock.XPosition * BlockManager.BLOCK_SIZE, 
            savedBlock.YPosition * BlockManager.BLOCK_SIZE);
        CreateItemPickup(savedBlock.BlockType, position);
    }


    private void CreateItemPickup(InventoryItemType inventoryItemType, Vector2 position) {
        GodotDictionary itemPickupData = inventoryItemType.ToDictionary();
        Rpc(nameof(CreateItemPickupOnPeer), itemPickupData, position);
    }

    [Rpc(CallLocal = true)]
    private void CreateItemPickupOnPeer(GodotDictionary itemPickupData, Vector2 position) {
        InventoryItemType inventoryItemType = InventoryItemType.Deserialize(itemPickupData);

        SavedItemPickup savedItemPickup = new(inventoryItemType, position);
        AddItemPickupToLocation(savedItemPickup);
        EmitSignal(SignalName.SavedItemPickupCreated, savedItemPickup);
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