using System;
using System.Globalization;
using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Scenes.Scripts;

public partial class Player : CharacterBody2D {
    [Export] private float speed = 300f;
    [Export] private MultiplayerSynchronizer positionSynchronizer;
    [Export] private float gravityCoefficient = 1600;
    [Export] private float jumpStrength = 800;
    [Export] private Camera2D camera;

    private int horizontalInput;
    private bool isFalling;
    private float xVelocity;
    private float yVelocity;

    [Signal]
    public delegate void LocalPlayerMovedEventHandler(
        int xCoords, int yCoords, int prevXCoords, int prevYCoords);

    [Signal]
    public delegate void LocalPlayerClickedEventHandler(int x, int y, string blockResourcePath);

    public static Player LocalPlayer { get; private set; }
    private int XCoords => (int)Math.Round(Position.X / WorldManager.BLOCK_SIZE);
    private int YCoords => (int)Math.Round(Position.Y / WorldManager.BLOCK_SIZE);

    public override void _EnterTree() {
        int peerId = Name.ToString()!.ToInt();
        positionSynchronizer.SetMultiplayerAuthority(peerId);
        if (peerId != Multiplayer.GetUniqueId()) return;

        LocalPlayer = this;
        camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;
        InputManager.Instance.MouseClicked += LogCellUnderMouse;
        PlayerManager.Instance.CreatedLocalPlayerOnServer += serverPosition => { Position = serverPosition; };
    }

    public override void _PhysicsProcess(double delta) {
        if (this != LocalPlayer) return;

        (int previousXCoords, int previousYCoords) = (XCoords, YCoords);

        isFalling = !TestMove(Transform, new Vector2(0, 0.1f));

        xVelocity = speed * horizontalInput;
        if (isFalling) {
            yVelocity += (float)delta * gravityCoefficient;
        } else {
            yVelocity = Math.Min(0, yVelocity);
        }

        Velocity = new Vector2(xVelocity, yVelocity);
        MoveAndSlide();

        if (previousXCoords != XCoords || previousYCoords != YCoords) {
            EmitSignal(SignalName.LocalPlayerMoved,
                XCoords, YCoords, previousXCoords, previousYCoords);
        }
    }

    private void LogCellUnderMouse(Vector2 vector) {
        Vector2 mousePos = GetGlobalMousePosition();
        int xPosition = (int)Math.Round(mousePos.X / WorldManager.BLOCK_SIZE);
        int yPosition = (int)Math.Round(mousePos.Y / WorldManager.BLOCK_SIZE);
        // EmitSignal(SignalName.LocalPlayerClicked, xPosition, yPosition, "res://Resources/BlockType/Stone.tres");
    }

    private void OnJumpPressed() {
        if (isFalling) return;
        isFalling = true;
        yVelocity = -jumpStrength;
    }
}