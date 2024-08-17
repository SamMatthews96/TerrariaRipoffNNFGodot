using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.Managers.Host;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.UI;
using TerrariaRipoffNNF.Scripts.Utils;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class Player : CharacterBody2D {
    [Export] private MultiplayerSynchronizer positionSynchronizer;
    [Export] private Camera2D camera;
    [Export] private Dictionary _playerInfoDictionary;
    [Export] private Area2D _pickupArea;
    [Export] private PackedScene _packedUi;
    [Export] public Inventory Inventory { get; private set; }

    [Export] private float speed = 300f;
    [Export] private float gravityCoefficient = 1600;
    [Export] private float jumpStrength = 800;
    [Export] private Vector2 _spawnPosition;

    private int horizontalInput;
    private bool isFalling;
    private float xVelocity;
    private float yVelocity;

    private IntVector _previousCoords;

    private IntVector Coords => new(
        (int)Math.Round(Position.X / GameManager.BlockSize),
        (int)Math.Round(Position.Y / GameManager.BlockSize));

    private int PeerId => Name.ToString().ToInt();
    private bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;

    [Signal] public delegate void MovedCellEventHandler(Dictionary positionChange);

    [Signal] public delegate void PickedUpItemEventHandler(ActivePickup activePickup);

    public override void _Ready() {
        Position = _spawnPosition;

        if (GameManager.Instance.IsHost) {
            _pickupArea.BodyEntered += OnServerCollidedWithPickup;
        }

        if (!IsLocalPlayer) return;
        camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged += newInput => horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;
        
        UiManager gameUi = _packedUi.Instantiate<UiManager>();
        gameUi.Initialize(this);
        GameManager.Instance.AddChild(gameUi);
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

    private void OnJumpPressed() {
        if (isFalling) return;
        isFalling = true;
        yVelocity = -jumpStrength;
    }

    private void OnServerCollidedWithPickup(Node node) {
        if (node is not ActivePickup activePickup) {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }

        bool success = Inventory.TryAddItems(activePickup.SavedPickup.InventoryItems);
        if (success) {
            EmitSignal(SignalName.PickedUpItem, activePickup);
        }
    }
}