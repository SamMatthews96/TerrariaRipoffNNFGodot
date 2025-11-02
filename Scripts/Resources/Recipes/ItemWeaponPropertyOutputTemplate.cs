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
        float speed = Speed.ResolveTemplate(suppliedIngredients);
        float damage = Damage.ResolveTemplate(suppliedIngredients);
        Texture2D texture = Texture.ResolveTemplate(suppliedIngredients);
        
        return ItemWeapon.Create(
            speed: speed,
            damage: damage,
            texture: texture,
            weaponType: WeaponType
        );
    }
}