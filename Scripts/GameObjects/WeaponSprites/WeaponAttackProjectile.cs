using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class WeaponAttackProjectile : WeaponAttackNode {
    private Vector2 _velocity;

    [Export] private Timer _timer;

    public override void _Ready() {
        Position = Player.Position + new Vector2(22, -50);
        
        Vector2 delta = TargetPosition - Position;
        Rotation = (float)Math.Atan2(delta.Y, delta.X);

        float speed = 1000;
        float xVelocity = speed * (float)Math.Cos(Rotation);
        float yVelocity = speed * (float)Math.Sin(Rotation);
        _velocity = new Vector2(xVelocity, yVelocity);
        
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