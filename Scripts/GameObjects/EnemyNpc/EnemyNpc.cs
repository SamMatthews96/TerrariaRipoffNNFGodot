using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

public partial class EnemyNpc : CharacterBody2D {
    private float _health = 100;
    [Export] private Area2D _hitbox;
    [Export] private ColorRect _healthBar;

    public static EnemyNpc Create(Vector2I spawnCoords) {
        EnemyNpc newEnemyNpc = Data.PackedScenes.TestNpc.Instantiate<EnemyNpc>();
        newEnemyNpc.Position = new Vector2(
            spawnCoords.X * Game.BlockSize,
            spawnCoords.Y * Game.BlockSize
        );

        return newEnemyNpc;
    }

    public override void _Ready() {
        _hitbox.AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area) {
        // if (area is WeaponAttackNode weaponAttackNode) {
        //     _health -= weaponAttackNode.Weapon.Damage;
        //     if (_health <= 0) {
        //         QueueFree();
        //     }
        // }
    }
}