using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemWeapon : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Damage { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    [Export] public WeaponType WeaponType { get; private set; }

    public static ItemWeapon Create(
        float speed, float damage, Texture2D texture, WeaponType weaponType) {
        return new ItemWeapon {
            Speed = speed,
            Damage = damage,
            Texture = texture,
            WeaponType = weaponType
        };
    }


    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Weapon");
        tooltipAttributes.Add("Speed", Speed);
        tooltipAttributes.Add("Damage", Damage);
        return tooltipAttributes;
    }
}