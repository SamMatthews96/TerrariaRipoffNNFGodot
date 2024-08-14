using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

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

    private IntVector Coords => new(
        (int)Math.Round(Position.X / GameManager.BlockSize),
        (int)Math.Round(Position.Y / GameManager.BlockSize));

    private IntVector _previousCoords;

    private int PeerId => Name.ToString().ToInt();
    private bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;

    [Signal] public delegate void MovedCellEventHandler(Dictionary positionChange);

    public override void _Ready() {
        Position = _spawnPosition;

        if (!IsLocalPlayer) return;
        camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;
        InputManager.Instance.MouseClicked += LogCellUnderMouse;
    }

    public void Initialize(int peerId, PlayerInfo playerInfo, Vector2 spawnPosition) {
        HostManager.RequireHost();

        Name = peerId.ToString();
        _playerInfoDictionary = playerInfo.Serialize();
        _spawnPosition = spawnPosition;
    }

    public override void _EnterTree() {
        positionSynchronizer.SetMultiplayerAuthority(PeerId);
    }

    public override void _PhysicsProcess(double delta) {
        if (Multiplayer.GetUniqueId() != PeerId) return;

        _previousCoords = Coords;
        isFalling = !TestMove(Transform, new Vector2(0, 0.1f));
        xVelocity = speed * horizontalInput;
        if (isFalling) {
            yVelocity += (float)delta * gravityCoefficient;
        } else {
            yVelocity = Math.Min(0, yVelocity);
        }

        Velocity = new Vector2(xVelocity, yVelocity);
        MoveAndSlide();

        if (_previousCoords == Coords) return;
        Dictionary positionChange = new() {
            { "X", Coords.X },
            { "Y", Coords.Y },
            { "PreviousX", _previousCoords.X },
            { "PreviousY", _previousCoords.Y }
        };
        RpcId(Manager.MultiplayerHostId, nameof(ServerMovedCell), positionChange);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerMovedCell(Dictionary positionChange) {
        EmitSignal(SignalName.MovedCell, positionChange);
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