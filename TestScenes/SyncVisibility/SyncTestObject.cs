using Godot;

namespace TerrariaRipoffNNF.testScenes.SyncVisibility;

public partial class SyncTestObject : Node2D {

    private Vector2 _velocity = new(200, 150);
    private Vector2 _screenSize;
    [Export] public MultiplayerSynchronizer Sync { get; private set; }

    public override void _Ready() {
        _screenSize = GetViewportRect().Size;
    }

    public override void _Process(double delta) {
        if (!Multiplayer.IsServer()) return;

        Position += _velocity * (float)delta;

        // Bounce off edges
        if (Position.X <= 0 || Position.X >= _screenSize.X) {
            _velocity.X = -_velocity.X;
        }
        if (Position.Y <= 0 || Position.Y >= _screenSize.Y) {
            _velocity.Y = -_velocity.Y;
        }

        // Keep within bounds
        Position = new Vector2(
            Mathf.Clamp(Position.X, 0, _screenSize.X),
            Mathf.Clamp(Position.Y, 0, _screenSize.Y)
        );
    }
}