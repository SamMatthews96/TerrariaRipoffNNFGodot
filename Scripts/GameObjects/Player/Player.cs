using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    public static Player Create(int peerId , Dictionary playerData) {
        BeforePlayerSpawned?.Invoke(playerData);
        Player player = Data.PackedScenes.Player.Instantiate<Player>();
        player.Name = peerId.ToString();

        player._spawnPosition = Manager.Instance.Game.DefaultSpawnPosition;
        PlayerSpawned?.Invoke(player);
        return player;
    }

    [Export] public Inventory Inventory { get; private set; }
    [Export] public ActionController ActionController { get; private set; }
    [Export] public PickupArea PickupArea { get; private set; }
    [Export] public PlayerEquipment PlayerEquipment { get; private set; }
    [Export] public Crafting Crafting { get; private set; }

    [Export] private MultiplayerSynchronizer _positionSynchronizer;
    [Export] private Camera2D _camera;
    [Export] private Area2D _pickupArea;
    [Export] private float _speed = 300f;
    [Export] private float _gravityCoefficient = 1600;
    [Export] private float _jumpStrength = 800;
    [Export] private Vector2 _spawnPosition;

    private int _horizontalInput;
    private bool _isFalling;
    private float _xVelocity;
    private float _yVelocity;

    private IntVector _previousCoords;

    public IntVector Coords => new(Position / Game.BlockSize);

    private int PeerId => Name.ToString().ToInt();
    public bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;

    public static event Action<Dictionary> BeforePlayerSpawned;
    
    public static event Action<Player> BeforeLocalPlayerSpawned;
    public static event Action<Player> PlayerSpawned;
    public event Action<Dictionary> MovedCell;

    public event Action BeforePlayerLeaveScene;

    #region Creation

    public override void _EnterTree() {
        _positionSynchronizer.SetMultiplayerAuthority(PeerId);
        if (IsLocalPlayer) {
            BeforeLocalPlayerSpawned?.Invoke(this);
        }
    }

    public override void _Ready() {
        Position = _spawnPosition;

        if (IsLocalPlayer) {
            InitializeLocalPlayer();
        }
    }

    public override void _ExitTree() {
        if (IsLocalPlayer) {
            Manager.Instance.Game.InputManager.HorizontalInputChanged -= OnHorizontalInputChanged;
            Manager.Instance.Game.InputManager.JumpPressed -= OnJumpPressed;
        }

        BeforePlayerLeaveScene?.Invoke();
    }

    private void InitializeLocalPlayer() {
        _camera.Enabled = true;
        Manager.Instance.Game.InputManager.HorizontalInputChanged += OnHorizontalInputChanged;
        Manager.Instance.Game.InputManager.JumpPressed += OnJumpPressed;
    }

    private void OnHorizontalInputChanged(int newInput) {
        _horizontalInput = newInput;
    }

    #endregion

    public override void _PhysicsProcess(double delta) {
        if (Multiplayer.GetUniqueId() != PeerId) return;

        _previousCoords = Coords;
        _isFalling = !TestMove(Transform, new Vector2(0, 0.1f));
        _xVelocity = _speed * _horizontalInput;
        if (_isFalling) {
            _yVelocity += (float)delta * _gravityCoefficient;
        }
        else {
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
}