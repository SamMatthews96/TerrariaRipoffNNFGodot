using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemWeapon : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Power { get; private set; }

    public static ItemWeapon Create(float speed, float power) {
        return new ItemWeapon {
            Speed = speed,
            Power = power
        };
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Weapon");
        tooltipAttributes.Add("Speed", Speed);
        tooltipAttributes.Add("Power", Power);
        return tooltipAttributes;
    }
}