namespace TerrariaRipoffNNF;

public class InventoryItems : StackedItems {
    public InventoryItems(ItemType itemType, int count = 1) : base(itemType, count) {
        ItemType = itemType;
        Count = count;
    }

    public void AddItems(int count) {
        Count += count;
    }

    public void RemoveItems(int count) {
        Count -= count;
    }
}