using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemWeapon : ItemEquipment {
    [Export] public float AttackRate { get; private set; }
    [Export] public float Damage { get; private set; }
    [Export] public PackedScene Scene { get; private set; }

    public ItemWeapon() { }

    public ItemWeapon(
        ItemWeaponOutputTemplate template,
        Dictionary<string, Item> suppliedIngredients
    ) {
        Scene = template.Scene;
        AttackRate = template.Speed.ResolveTemplate(suppliedIngredients);
        Damage = template.Damage.ResolveTemplate(suppliedIngredients);
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("PropertyName", "Weapon");
        tooltipAttributes.Add("Attack Rate", AttackRate);
        tooltipAttributes.Add("Damage", Damage);
        return tooltipAttributes;
    }
}