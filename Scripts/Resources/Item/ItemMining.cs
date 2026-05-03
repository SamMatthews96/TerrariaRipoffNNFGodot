using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemMining : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public int Range { get; private set; }
    [Export] public float Power { get; private set; }

    public static ItemMining Create(float speed, int range, float power) {
        return new ItemMining {
            Speed = speed,
            Range = range,
            Power = power,
            Slot = EquipmentSlot.Mining
        };
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Mining");
        tooltipAttributes.Add("Speed", Speed);
        tooltipAttributes.Add("Range", Range);
        tooltipAttributes.Add("Power", Power);
        return tooltipAttributes;
    }
}