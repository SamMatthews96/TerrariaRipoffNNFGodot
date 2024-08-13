using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Scenes.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.GameObjects.Scripts;

public partial class Player : CharacterBody2D {
    [Export] private MultiplayerSynchronizer positionSynchronizer;
    [Export] private Camera2D camera;
    [Export] private Dictionary _playerInfoDictionary;

    [Export] private float speed = 300f;
    [Export] private float gravityCoefficient = 1600;
    [Export] private float jumpStrength = 800;
    [Export] private Vector2 _spawnPosition;

    private int horizontalInput;
    private bool isFalling;
    private float xVelocity;
    private float yVelocity;

    public int XCoords => (int)Math.Round(Position.X / GameManager.BlockSize);
    public int YCoords => (int)Math.Round(Position.Y / GameManager.BlockSize);
    public int PreviousXCoords { get; private set; }
    public int PreviousYCoords { get; private set; }

    private int PeerId => Name.ToString().ToInt();
    private bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;

    [Signal] public delegate void LocalPlayerMovedEventHandler(Player player);

    public override void _Ready() {
        Position = _spawnPosition;

        if (!IsLocalPlayer) return;
        camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;
        InputManager.Instance.MouseClicked += LogCellUnderMouse;
    }

    public void Initialize(int peerId, PlayerInfo playerInfo, Vector2 spawnPosition) {
        Name = peerId.ToString();
        _playerInfoDictionary = playerInfo.Serialize();
        _spawnPosition = spawnPosition;
    }


    public override void _EnterTree() {
        positionSynchronizer.SetMultiplayerAuthority(PeerId);
    }

    public override void _PhysicsProcess(double delta) {
        if (Multiplayer.GetUniqueId() != PeerId) return;
        PreviousXCoords = XCoords;
        PreviousYCoords = YCoords;

        isFalling = !TestMove(Transform, new Vector2(0, 0.1f));

        xVelocity = speed * horizontalInput;
        if (isFalling) {
            yVelocity += (float)delta * gravityCoefficient;
        } else {
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
        int xPosition = (int)Math.Round(mousePos.X / GameManager.BlockSize);
        int yPosition = (int)Math.Round(mousePos.Y / GameManager.BlockSize);
        // EmitSignal(SignalName.LocalPlayerClicked, xPosition, yPosition, "res://Resources/BlockType/Stone.tres");
    }

    private void OnJumpPressed() {
        if (isFalling) return;
        isFalling = true;
        yVelocity = -jumpStrength;
    }
}