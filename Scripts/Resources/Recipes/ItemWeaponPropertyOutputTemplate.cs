using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemWeaponPropertyOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipePropertyMapMultiplier Speed { get; private set; }
    [Export] public RecipePropertyMapMultiplier Damage { get; private set; }
    [Export] public RecipePropertyMapTexture Texture { get; private set; }
    [Export] public WeaponType WeaponType { get; private set; }

    public override ItemWeapon Build(
        Dictionary<string, Item> suppliedIngredients,
        Dictionary<string, RecipeIngredientSlot> ingredientSlots
    ) {
        return ItemWeapon.Create(
            speed: Speed.ResolveTemplate(suppliedIngredients),
            damage: Damage.ResolveTemplate(suppliedIngredients),
            texture: Texture.ResolveTemplate(suppliedIngredients),
            weaponType: WeaponType
        );
    }
}