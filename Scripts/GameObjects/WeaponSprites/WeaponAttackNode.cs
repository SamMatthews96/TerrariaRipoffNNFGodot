using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class WeaponAttackNode : Area2D {
    protected ItemWeapon Weapon { get; private set; }
    protected Player Player { get; private set; }
    protected Vector2 TargetPosition { get; private set; }

    public static WeaponAttackNode Create(
        ItemWeapon weapon, // damage, and other potential properties
        Player player, // spawn position
        Vector2 targetPosition // target position) {
    ) {
        WeaponAttackNode newNode =
            weapon.PackedWeaponAttackNode.Instantiate<WeaponAttackNode>();
        newNode.Weapon = weapon;
        newNode.Player = player;
        newNode.TargetPosition = targetPosition;
        return newNode;
    }
}