using Godot.Collections;

namespace TerrariaRipoffNNF;

public static class PlayerCreator {
    public static Dictionary CreatePlayer(string name) {
        Dictionary newPlayer = new();
        newPlayer.Add("Name", name);
        
        Dictionary inventory = new();
        inventory.Add("InventoryItemsList", new Array());
        
        newPlayer.Add("Inventory", inventory);
        FileManager.SavePlayer(newPlayer);
        return newPlayer;
    }
}