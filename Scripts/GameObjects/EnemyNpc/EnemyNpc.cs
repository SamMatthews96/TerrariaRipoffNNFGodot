using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

public partial class EnemyNpc : CharacterBody2D {
    [Export] private float _health = 10;
    [Export] private Area2D _hitbox;
    [Export] private ProgressBar _healthBar;

    public event Action<EnemyNpc> HitZeroHealth;

    public static EnemyNpc Create(Vector2I spawnCoords, int id) {
        EnemyNpc newEnemyNpc = Data.PackedScenes.TestNpc.Instantiate<EnemyNpc>();
        newEnemyNpc.Position = new Vector2(
            spawnCoords.X * Game.BlockSize,
            spawnCoords.Y * Game.BlockSize
        );
        newEnemyNpc.Name = id.ToString();

        return newEnemyNpc;
    }

    public override void _Ready() {
        if (!Multiplayer.IsServer()) return;
        
        _hitbox.AreaEntered += OnAreaEntered;
        _healthBar.MaxValue = _health;
        _healthBar.Value = _health;
        TreeExiting += () => {
            _hitbox.AreaEntered -= OnAreaEntered;
            
        };
    }

    private void OnAreaEntered(Area2D area) {
        if (area is not WeaponAttackNode node) return;
        _health -= node.Weapon.Damage;
        _healthBar.Value = _health;
        if (_health <= 0) {
            HitZeroHealth?.Invoke(this);
        }
    }
}