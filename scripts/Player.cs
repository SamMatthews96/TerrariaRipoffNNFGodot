using System;
using Godot;
using static Godot.GD;

namespace TerrariaRipoffNNF.scripts;

public partial class Player : CharacterBody2D {
    
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
        OnPlayerSpawned += (sender, args) => { Print("player spawned"); };
        OnPlayerSpawned?.Invoke(this,EventArgs.Empty);
    }
    
    

    public override void _PhysicsProcess(double delta) {
        Vector2 velocity = Velocity;

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
        // delete this
        OnPlayerMovedCell?.Invoke(this,new OnPlayerMovedCellEventArgs {
            Direction = Direction.Down
        });
        // Print(Position.X,Position.Y);
    }
}