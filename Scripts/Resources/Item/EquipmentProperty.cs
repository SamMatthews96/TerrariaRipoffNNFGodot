using Godot;

namespace TerrariaRipoffNNF;

public abstract partial class EquipmentProperty : Resource {
    public enum SlotType {
        Mining,
        Building,
        Weapon,
        Head
    }

    public SlotType Slot { get; protected set; }
}