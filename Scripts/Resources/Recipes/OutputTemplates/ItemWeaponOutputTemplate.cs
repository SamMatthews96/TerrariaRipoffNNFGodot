using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ItemWeaponOutputTemplate : ItemPropertyOutputTemplate {
    [Export] public RecipeFieldMapFloat Speed { get; private set; }
    [Export] public RecipeFieldMapFloat Damage { get; private set; }
    [Export] public PackedScene Scene { get; private set; }

    public override ItemProperty Build(Dictionary<string, Item> suppliedIngredients) {
        return new ItemWeapon(this, suppliedIngredients);
    }
}