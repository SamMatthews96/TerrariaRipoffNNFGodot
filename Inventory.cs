using System;
using System.Collections.Generic;
using TerrariaRipoffNNF.scripts.Resources;

namespace TerrariaRipoffNNF; 

public class Inventory {
    public float MaxSpace { get; private set; }
    public readonly List<InventoryItem> InventoryItems = new();

    public Inventory(float maxSpace = 100) {
        MaxSpace = maxSpace;
    }

    public void AddItem(ItemResource newItemResource, int delta) {
        if (!newItemResource.IsStackable) {
            InventoryItems.Add(new InventoryItem(newItemResource, delta));
        }
        int currentItemIndex = InventoryItems.FindIndex(item => item.ItemResource == newItemResource);
        if (currentItemIndex == -1) {
            InventoryItems.Add(new InventoryItem(newItemResource, delta));
        } else {
            InventoryItems[currentItemIndex].IncreaseQuantity(delta);
        }
    }

    public void RemoveItems(ItemResource newItemResource, int delta) {
        int currentItemIndex = InventoryItems.FindIndex(item => item.ItemResource == newItemResource);
        if (currentItemIndex == -1) {
            throw new Exception("tried to remove inventory item that doesn't exist");
        }
        InventoryItems[currentItemIndex].DecreaseQuantity(delta);
        switch (InventoryItems[currentItemIndex].Quantity) {
            case < 0:
                throw new Exception("tried to remove more inventory items than exist");
            case 0:
                InventoryItems.RemoveAt(currentItemIndex);
                break;
        }
    }
    
    
    
    public class InventoryItem {
        public ItemResource ItemResource { get; private set; }
        public int Quantity { get; private set; }

        public InventoryItem(ItemResource itemResource, int quantity) {
            ItemResource = itemResource;
            Quantity = quantity;
        }

        public void IncreaseQuantity(int delta) {
            Quantity += delta;
        }

        public void DecreaseQuantity(int delta) {
            Quantity -= delta;
        }
    }
}