using System;
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class Player : CharacterBody2D {
    [Export] private MultiplayerSynchronizer multiplayerSynchronizer;
    public static event EventHandler OnPlayerSpawned;
    public event EventHandler<OnPlayerMovedCellEventArgs> OnPlayerMovedCell;

    public class OnPlayerMovedCellEventArgs : EventArgs {
        public Direction Direction;
    }

    public const float SPEED = 300.0f;
    public const float JUMP_VELOCITY = -400.0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _Ready() {
        multiplayerSynchronizer.SetMultiplayerAuthority(int.Parse(Name));
        Print("_Ready");

        Print(Name);
        OnPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    public override void _PhysicsProcess(double delta) {
        Vector2 velocity = Velocity;
        Vector2 startPosition = new Vector2(Position.X, Position.Y);

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") /*&& IsOnFloor()*/)
            velocity.Y = JUMP_VELOCITY;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 direction = Input.GetVector("runLeft", "runRight", "ui_up", "ui_down");
        if (direction != Vector2.Zero) {
            velocity.X = direction.X * SPEED;
        } else {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, SPEED);
        }

        Velocity = velocity;
        MoveAndSlide();
        Vector2 endPosition = Position;

        if (GameCoordinateToCellCoordinate(startPosition.Y) !=
            GameCoordinateToCellCoordinate(endPosition.Y) ||
            GameCoordinateToCellCoordinate(startPosition.X) !=
            GameCoordinateToCellCoordinate(endPosition.X)) {
            OnPlayerMovedCell?.Invoke(this, new OnPlayerMovedCellEventArgs {
                Direction = Direction.Down
            });
        }
    }

    public (int xPosition, int yPosition) GetCellPosition() {
        int xPosition = GameCoordinateToCellCoordinate(GlobalPosition.X);
        int yPosition = -GameCoordinateToCellCoordinate(GlobalPosition.Y);
        return (xPosition, yPosition);
    }

    private static int GameCoordinateToCellCoordinate(float coordinate) {
        return (int)Math.Round(coordinate / World.BLOCK_SIZE);
    }
}