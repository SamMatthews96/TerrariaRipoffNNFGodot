using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    [Export] private MultiplayerSynchronizer _positionSynchronizer;
    [Export] private Camera2D _camera;
    [Export] private Area2D _pickupArea;
    [Export] private Inventory _inventory;

    [Export] private float _speed = 300f;
    [Export] private float _gravityCoefficient = 1600;
    [Export] private float _jumpStrength = 800;
    [Export] private Vector2 _spawnPosition;

    [Export] private ActionState _gatherActionState;
    [Export] public ActionController ActionController { get; private set; }

    private int _horizontalInput;
    private bool _isFalling;
    private float _xVelocity;
    private float _yVelocity;

    private IntVector _previousCoords;

    private IntVector Coords => new(Position / Game.BlockSize);

    private int PeerId => Name.ToString().ToInt();
    public bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;

    public static event Action<Player> BeforeLocalPlayerSpawned;
    public event Action<Dictionary> MovedCell;
    public event Action<ActivePickup> PickedUpItem;
    public event Action<IntVector, float> GatherAttempted;

    #region Creation

    public override void _EnterTree() {
        _positionSynchronizer.SetMultiplayerAuthority(PeerId);

        BeforeLocalPlayerSpawned?.Invoke(this);
    }

    public override void _Ready() {
        Position = _spawnPosition;

        if (Manager.Instance.Game.IsHost) {
            //@todo this can be done inside the pickuparea node, add a script to it.
            _pickupArea.BodyEntered += OnServerCollidedWithPickup;
        }

        if (IsLocalPlayer) {
            InitializeLocalPlayer();
        }
    }

    private void InitializeLocalPlayer() {
        Manager.Instance.Game.Interface.InventoryUi.Initialize(_inventory);

        _camera.Enabled = true;
        InputManager.Instance.HorizontalInputChanged +=
            newInput => _horizontalInput = newInput;
        InputManager.Instance.JumpPressed += OnJumpPressed;

        _gatherActionState.PrimaryActionStarted += OnGatherStartAction;
    }

    #endregion

    public override void _PhysicsProcess(double delta) {
        if (Multiplayer.GetUniqueId() != PeerId) return;

        _previousCoords = Coords;
        _isFalling = !TestMove(Transform, new Vector2(0, 0.1f));
        _xVelocity = _speed * _horizontalInput;
        if (_isFalling) {
            _yVelocity += (float)delta * _gravityCoefficient;
        } else {
            _yVelocity = Math.Min(0, _yVelocity);
        }

        Velocity = new Vector2(_xVelocity, _yVelocity);
        MoveAndSlide();

        if (_previousCoords == Coords) return;
        Dictionary positionChange = new() {
            { "X", Coords.X },
            { "Y", Coords.Y },
            { "PreviousX", _previousCoords.X },
            { "PreviousY", _previousCoords.Y }
        };
        RpcId(Manager.HostId, nameof(ServerMovedCell), positionChange);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void ServerMovedCell(Dictionary positionChange) {
        MovedCell?.Invoke(positionChange);
    }

    private void OnJumpPressed() {
        if (_isFalling) return;
        _isFalling = true;
        _yVelocity = -_jumpStrength;
    }

    private void OnServerCollidedWithPickup(Node node) {
        if (node is not ActivePickup activePickup) {
            throw new Exception("[20240816.0934.1] Pickup area collision with non-pickup");
        }

        bool success = _inventory.TryAddItems(activePickup.SavedPickup.InventoryItems);
        if (success) {
            PickedUpItem?.Invoke(activePickup);
        }
    }

    private void OnGatherStartAction(Vector2 mouseWorldPosition) {
        RpcId(Manager.HostId, nameof(HostGatherStartAction),
            mouseWorldPosition);
        // @todo this should occur inside the ActionState item
        // it should pass player and mouseWorldPosition as arguments
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    private void HostGatherStartAction(Vector2 mouseWorldPosition) {
        IntVector coords = new(mouseWorldPosition / Game.BlockSize);
        if (!coords.IsInBounds()) return;
        GatherAttempted?.Invoke(coords, 100f);
    }
}