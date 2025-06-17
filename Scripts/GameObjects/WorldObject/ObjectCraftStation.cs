using Godot;

namespace TerrariaRipoffNNF;

public partial class ObjectCraftStation : ObjectProperty {
    public CraftingStationType Type { get; private set; }

    public ObjectCraftStation(
        WorldObject worldObject, ItemCraftStation craftStation) : base(worldObject) {
        
        Type = craftStation.Type;
    }

    public override void Init() {
        CraftStationArea area = CraftStationArea.Create(this);
        WorldObject.ParentNode.AddChild(area);
    }
}