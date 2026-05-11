using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemWeaponOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipeFieldMapFloat Speed { get; private set; }
    [Export] public RecipeFieldMapFloat Damage { get; private set; }
    [Export] public RecipeFieldMapTexture TextureSingle { get; private set; }

    public override ItemWeapon Build(
        Dictionary<string, Item> suppliedIngredients
    ) {
        throw new NotImplementedException();
        // float speed = Speed.ResolveTemplate(suppliedIngredients);
        // float damage = Damage.ResolveTemplate(suppliedIngredients);
        // Texture2D texture = TextureSingle.ResolveTemplate(suppliedIngredients);
        //
        // return ItemWeapon.Create(
        //     speed: speed,
        //     damage: damage,
        //     texture: texture,
        //     weaponType: WeaponType
        // );
    }
}