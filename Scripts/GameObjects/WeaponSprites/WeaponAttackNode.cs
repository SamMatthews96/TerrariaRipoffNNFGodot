using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class WeaponAttackNode : Area2D {
    public ItemWeapon Weapon { get; private set; }
    protected Player Player { get; private set; }
    protected Vector2 TargetPosition { get; private set; }

    public static WeaponAttackNode Create(
        ItemWeapon weapon, Player player, Vector2 targetPosition
    ) {
        WeaponAttackNode newNode = weapon.WeaponType switch {
            WeaponType.MeleeSwing =>
                Data.PackedScenes.WeaponSwing.Instantiate<WeaponAttackNode>(),
            WeaponType.Projectile =>
                Data.PackedScenes.WeaponProjectile.Instantiate<WeaponAttackNode>(),
            _ => throw new ArgumentOutOfRangeException(nameof(weapon))
        };

        newNode.Weapon = weapon;
        newNode.Player = player;
        newNode.TargetPosition = targetPosition;
        return newNode;
    }
}