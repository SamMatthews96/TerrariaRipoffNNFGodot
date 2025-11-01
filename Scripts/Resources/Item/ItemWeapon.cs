using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemWeapon : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Damage { get; private set; }
    [Export] public Texture2D Texture { get; private set; }
    [Export] public PackedScene PackedWeaponAttackNode { get; private set; }

    public static ItemWeapon Create(
        float speed, float damage, Texture2D texture, PackedScene packedScene) {
        return new ItemWeapon {
            Speed = speed,
            Damage = damage,
            Texture = texture,
            PackedWeaponAttackNode = packedScene
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