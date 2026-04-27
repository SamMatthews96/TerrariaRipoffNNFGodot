using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemWeaponOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipeFieldMapFloat Speed { get; private set; }
    [Export] public RecipeFieldMapFloat Damage { get; private set; }
    [Export] public RecipeFieldMapTexture TextureSingle { get; private set; }
    [Export] public WeaponType WeaponType { get; private set; }

    public override ItemWeapon Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        float speed = Speed.ResolveTemplate(suppliedIngredients);
        // float speed = 7;
        float damage = Damage.ResolveTemplate(suppliedIngredients);
        // float damage = 7;
        Texture2D texture = TextureSingle.ResolveTemplate(suppliedIngredients);
        
        return ItemWeapon.Create(
            speed: speed,
            damage: damage,
            texture: texture,
            weaponType: WeaponType
        );
    }
}