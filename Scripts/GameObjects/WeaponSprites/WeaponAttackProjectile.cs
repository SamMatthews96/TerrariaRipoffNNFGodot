using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.WeaponSprites;

public partial class WeaponAttackProjectile : WeaponAttackNode {
    private Vector2 _velocity;
    private Vector2 _centerOffset = new(22, -50);
    
    public Vector2I Coords => (Vector2I)(Position / Game.BlockSize);

    public event Action<Vector2I, Vector2I> MovedCell;
    public event Action Timeout {
        add => _timer.Timeout += value;
        remove => _timer.Timeout -= value;
    }
    
    
    [Export] private Timer _timer;

    public override void _Ready() {
        Position = Player.Position + _centerOffset;
        
        Vector2 delta = TargetPosition - Position;
        Rotation = (float)Math.Atan2(delta.Y, delta.X);

        float speed = 1000;
        float xVelocity = speed * (float)Math.Cos(Rotation);
        float yVelocity = speed * (float)Math.Sin(Rotation);
        _velocity = new Vector2(xVelocity, yVelocity);
        
        _timer.Start();
    }

    private void OnTimeout() {
        QueueFree();
    }

    public override void _PhysicsProcess(double delta) {
        Vector2I previousCoords = Coords;
        Position += _velocity * (float)delta;
        _velocity += new Vector2(0, 1000) * (float)delta;
        Rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
        if (previousCoords == Coords) return;
        MovedCell?.Invoke(Coords, previousCoords);
    }
}