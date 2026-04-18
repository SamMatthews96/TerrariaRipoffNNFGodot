using Godot;

namespace TerrariaRipoffNNF;

public partial class NetworkTestScene : Node2D {
    private const float Speed = 200f;
    private Vector2 _targetPosition;
    private readonly RandomNumberGenerator _rng = new();
    

    public override void _Ready() {
        if (!Multiplayer.IsServer()) return;
        _rng.Randomize();
        SelectNewTarget();
    }

    public override void _Process(double delta) {
        if (!Multiplayer.IsServer()) return;
        
        var direction = (_targetPosition - Position).Normalized();
        var distance = Position.DistanceTo(_targetPosition);

        if (distance < Speed * delta) {
            Position = _targetPosition;
            SelectNewTarget();
        } else {
            Position += direction * Speed * (float)delta;
        }
    }

    private void SelectNewTarget() {
        var viewport = GetViewportRect();
        _targetPosition = new Vector2(
            _rng.RandfRange(0, viewport.Size.X),
            _rng.RandfRange(0, viewport.Size.Y)
        );
    }
}
