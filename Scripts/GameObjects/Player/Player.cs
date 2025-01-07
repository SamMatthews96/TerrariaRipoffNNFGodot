using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    public static Player Create(int peerId, Dictionary playerData) {
        BeforePlayerSpawned?.Invoke(playerData);
        Player player = Data.PackedScenes.Player.Instantiate<Player>();
        player.Name = peerId.ToString();
        PlayerSpawned?.Invoke(player);
        return player;
    }

    private Game _game;

    [Export] public Inventory Inventory { get; private set; }
    [Export] public ActionController ActionController { get; private set; }
    [Export] public PickupArea PickupArea { get; private set; }
    [Export] public PlayerEquipment PlayerEquipment { get; private set; }
    [Export] public Crafting Crafting { get; private set; }

    [Export] private MultiplayerSynchronizer _positionSynchronizer;
    [Export] private Camera2D _camera;
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

    public event Action<Player> BeforePlayerLeaveScene;

    #region Creation

    public override void _EnterTree() {
        _positionSynchronizer.SetMultiplayerAuthority(PeerId);
        if (IsLocalPlayer) {
            _camera.Enabled = true;
            BeforeLocalPlayerSpawned?.Invoke(this);
        }
    }

    public override void _Ready() {
        Position = _spawnPosition;
    }

    public override void _ExitTree() {
        BeforePlayerLeaveScene?.Invoke(this);
    }

    public void InitAsHost(Game game) {
        Inventory.InitAsHost();
        PickupArea.InitAsHost();
        ActionController.InitAsHost(game);
    }

    public void InitAsLocal(Game game) {
        if (_game is not null) {
            throw new Exception("[20250104.0137.1] Game already set");
        }

        _game = game;
        _game.InputManager.HorizontalInputChanged += OnHorizontalInputChanged;
        _game.InputManager.JumpPressed += OnJumpPressed;
        TreeExiting += OnTreeExitingGame;

        ActionController.InitAsLocal(game);
        Crafting.InitAsLocal(game);
    }

    private void OnTreeExitingGame() {
        _game.InputManager.HorizontalInputChanged -= OnHorizontalInputChanged;
        _game.InputManager.JumpPressed -= OnJumpPressed;
        TreeExiting -= OnTreeExitingGame;
    }

    private void OnHorizontalInputChanged(int newInput) {
        _horizontalInput = newInput;
    }

    #endregion

    public override void _PhysicsProcess(double delta) {
        if (!IsLocalPlayer) return;

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
        RpcId(SceneManager.HostId, nameof(ServerMovedCell), positionChange);
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