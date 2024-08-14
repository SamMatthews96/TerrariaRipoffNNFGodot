using Godot;

namespace TerrariaRipoffNNF.Scripts.Resources; 

public partial class InventoryItem : Resource {
    public InventoryItemType Type { get; private set; }
    public int Amount { get; private set; }
    
    public InventoryItem(InventoryItemType type, int amount) {
        Type = type;
        Amount = amount;
    }
    
    public void Add(int amount) {
        Amount += amount;
    }
    
    public void Remove(int amount) {
        Amount -= amount;
    }
    
}