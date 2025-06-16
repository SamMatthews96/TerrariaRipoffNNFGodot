using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemEquipment : ItemProperty {
    public EquipmentSlot Slot { get; protected set; }
}