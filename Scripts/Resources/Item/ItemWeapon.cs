using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemWeapon : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Power { get; private set; }
    [Export] public PackedScene TestProjectile { get; private set; }
    
    public static ItemWeapon Create(float speed, float power) {
        return new ItemWeapon {
            Speed = speed,
            Power = power
        };
        /*
         * This has all the information required for the weapon sprite
         * that spawns. 
         */
    }

    
    

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Weapon");
        tooltipAttributes.Add("Speed", Speed);
        tooltipAttributes.Add("Power", Power);
        return tooltipAttributes;
    }
}