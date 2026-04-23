using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class NetworkTestScene : Node2D {
    private const float Speed = 200f;
    private Vector2 _targetPosition;
    private readonly RandomNumberGenerator _rng = new();

    [Export] private Timer _timer;
    [Export] private Dictionary _syncDict;

    public override void _Ready() {
        _timer.Timeout += OnTimeout;
        
        if (!Multiplayer.IsServer()) return;
        _rng.Randomize();
        SelectNewTarget();
        _syncDict = new Dictionary();
        _syncDict.Add("test", "test");
        _syncDict.Add("test2", 123);
        _syncDict.Add("test3", new Dictionary {{"test", "test"}});

    }

    private void OnTimeout() {
        GD.Print(Multiplayer.GetUniqueId());
        GD.Print(_syncDict);
    }

    public override void _ExitTree() {
        _timer.Timeout -= OnTimeout;
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
