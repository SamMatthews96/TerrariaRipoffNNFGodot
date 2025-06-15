using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldPickup : Node2D {
    public Item Item { get; private set; }
    public WorldObject WorldObject { get; private set; }
    
    public static WorldPickup Create(WorldObject worldObject, Item item) {
        WorldPickup pickup = Data.PackedScenes.WorldPickup.Instantiate<WorldPickup>();
        pickup.Item = item;
        pickup.WorldObject = worldObject;
        return pickup;
    }
}