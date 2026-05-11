using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemWeapon : ItemEquipment {
    [Export] public float AttackRate { get; private set; }
    [Export] public float Damage { get; private set; }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Weapon");
        tooltipAttributes.Add("Attack Rate", AttackRate);
        tooltipAttributes.Add("Damage", Damage);
        return tooltipAttributes;
    }
}