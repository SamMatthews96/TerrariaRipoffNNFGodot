using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class TestProjectile : RigidBody2D {
    public static TestProjectile Create(
        PackedScene projectileScene,
        Vector2 spawnPosition,
        Vector2 targetPosition
    ) {
        TestProjectile projectile = projectileScene.Instantiate<TestProjectile>();
        projectile.Position = spawnPosition;
        Vector2 delta = targetPosition - spawnPosition;
        double direction = Math.Atan2(delta.Y, delta.X);
        projectile.Rotation = (float)direction;
        
        float speed = 1500;
        float xVelocity = speed * (float)Math.Cos(direction);
        float yVelocity = speed * (float)Math.Sin(direction);
        projectile.LinearVelocity = new Vector2(xVelocity, yVelocity);
        
        return projectile;
    }
    
    [Export] private Timer _timer;
    
    public override void _Ready() {
        _timer.Start();
        _timer.Timeout += OnTimeout;
    }
    
    public override void _ExitTree() {
        _timer.Timeout -= OnTimeout;
    }

    private void OnTimeout() {
        QueueFree();
    }

    public override void _PhysicsProcess(double delta) {
        Rotation = (float)Math.Atan2(LinearVelocity.Y, LinearVelocity.X);
    }
}