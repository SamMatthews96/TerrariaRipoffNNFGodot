using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class TestProjectile : Area2D {
    private Vector2 _velocity;

    public static TestProjectile Create(
        PackedScene projectileScene,
        Vector2 spawnPosition,
        Vector2 targetPosition
    ) {
        TestProjectile projectile = projectileScene.Instantiate<TestProjectile>();
        projectile.Position = spawnPosition + new Vector2(22, -50);
        Vector2 delta = targetPosition - spawnPosition;
        double direction = Math.Atan2(delta.Y, delta.X);
        projectile.Rotation = (float)direction;

        float speed = 1000;
        float xVelocity = speed * (float)Math.Cos(direction);
        float yVelocity = speed * (float)Math.Sin(direction);
        projectile._velocity = new Vector2(xVelocity, yVelocity);

        return projectile;
    }

    [Export] private Timer _timer;

    public override void _Ready() {
        _timer.Start();
        _timer.Timeout += OnTimeout;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body) {
        QueueFree();

        if (body is WorldSolid) {
            // QueueFree();
        }
    }

    public override void _ExitTree() {
        _timer.Timeout -= OnTimeout;
    }

    private void OnTimeout() {
        QueueFree();
    }

    public override void _PhysicsProcess(double delta) {
        Position += _velocity * (float)delta;
        _velocity += new Vector2(0, 1000) * (float)delta;
        Rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
    }
}