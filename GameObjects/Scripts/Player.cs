using System;
using Godot;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class Player : CharacterBody2D{
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
    public delegate void LocalPlayerMovedEventHandler(Player player);

    // [Signal]
    // public delegate void LocalPlayerClickedEventHandler(int x, int y, string blockResourcePath);

    public static Player LocalPlayer { get; private set; }
    public int XCoords => (int)Math.Round(Position.X / BlockManager.BLOCK_SIZE);
    public int YCoords => (int)Math.Round(Position.Y / BlockManager.BLOCK_SIZE);
    public int PreviousXCoords { get; private set; }
    public int PreviousYCoords { get; private set; }
    public IntVector GridPosition => new(XCoords, YCoords);

    public override void _EnterTree() {
        int peerId = Name.ToString()!.ToInt();
        positionSynchronizer.SetMultiplayerAuthority(peerId);
        if (peerId != Multiplayer.GetUniqueId()) return;
        IntVector spawnPosition = WorldManager.Instance.GetPlayerSpawnPosition();

        Position = new Vector2(
            spawnPosition.X * BlockManager.BLOCK_SIZE, spawnPosition.Y * BlockManager.BLOCK_SIZE);

        LocalPlayer = this;
        camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;
        InputManager.Instance.MouseClicked += LogCellUnderMouse;
        
    }

    public override void _PhysicsProcess(double delta) {
        if (this != LocalPlayer) return;
        PreviousXCoords = XCoords;
        PreviousYCoords = YCoords;

        isFalling = !TestMove(Transform, new Vector2(0, 0.1f));

        xVelocity = speed * horizontalInput;
        if (isFalling) {
            yVelocity += (float)delta * gravityCoefficient;
        }
        else {
            yVelocity = Math.Min(0, yVelocity);
        }

        Velocity = new Vector2(xVelocity, yVelocity);
        MoveAndSlide();

        if (PreviousXCoords != XCoords || PreviousYCoords != YCoords) {
            EmitSignal(SignalName.LocalPlayerMoved, this);
        }
    }

    private void LogCellUnderMouse(Vector2 vector) {
        Vector2 mousePos = GetGlobalMousePosition();
        int xPosition = (int)Math.Round(mousePos.X / BlockManager.BLOCK_SIZE);
        int yPosition = (int)Math.Round(mousePos.Y / BlockManager.BLOCK_SIZE);
        // EmitSignal(SignalName.LocalPlayerClicked, xPosition, yPosition, "res://Resources/BlockType/Stone.tres");
    }

    private void OnJumpPressed() {
        if (isFalling) return;
        isFalling = true;
        yVelocity = -jumpStrength;
    }
}