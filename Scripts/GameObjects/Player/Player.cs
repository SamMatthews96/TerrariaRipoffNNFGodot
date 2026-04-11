using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    public static Player Create(int peerId, Vector2I spawnCoords) {
        Player player = Data.PackedScenes.Player.Instantiate<Player>();
        player.Name = peerId.ToString();
        player.SpawnCoords = spawnCoords;
        player.SpawnPosition = new Vector2(
            spawnCoords.X * Game.BlockSize,
            spawnCoords.Y * Game.BlockSize
        );
        return player;
    }

    public Inventory Inventory { get; private set; }
    public ActionController ActionController { get; private set; }
    public Crafting Crafting { get; private set; }
    public PickupArea PickupArea { get; private set; }
    [Export] public PlayerEquipment PlayerEquipment { get; private set; }

    [Export] private MultiplayerSynchronizer _positionSynchronizer;
    [Export] private Camera2D _camera;
    [Export] private float _speed = 300f;
    [Export] private float _gravityCoefficient = 1600;
    [Export] private float _jumpStrength = 800;
    [Export] public Vector2 SpawnPosition { get; private set; }
    public Vector2I SpawnCoords { get; private set; }


    public Game Game { get; private set; }
    private int _horizontalInput;
    private bool _isFalling;
    private float _xVelocity;
    private float _yVelocity;
    private string _characterName;

    public Vector2I Coords => new((int)(Position.X / Game.BlockSize), (int)(Position.Y / Game.BlockSize));

    public int PeerId => Name.ToString().ToInt();
    private bool _isLocalPlayer;


    public static event Action<Player> LocalPlayerSpawned;
    public event Action<Vector2I, Vector2I> MovedCell;

    public event Action<Player> PlayerDespawned;
    public static event Action PlayerSaved;

    public override void _EnterTree() {
        _positionSynchronizer.SetMultiplayerAuthority(PeerId);
        _isLocalPlayer = Multiplayer.GetUniqueId() == PeerId;
        if (_isLocalPlayer) {
            _camera.Enabled = true;
        }
    }

    public override void _Ready() {
        Position = SpawnPosition;
        foreach (int peer in Multiplayer.GetPeers()) {
            _positionSynchronizer.SetVisibilityFor(peer, true);
        }
    }

    public void AddPeerToSynchronizer(int peerId) {
        _positionSynchronizer.SetVisibilityFor(peerId, true);
    }

    public override void _ExitTree() {
        PlayerDespawned?.Invoke(this);
    }

    public void InitAsLocal(Game game, Dictionary playerData) {
        if (Game is not null) {
            throw new Exception("[20250104.0137.1] Game already set");
        }

        Inventory = Inventory.Create(game, playerData, this);
        ActionController = ActionController.Create(game, this);
        Crafting = Crafting.Create(game, this);
        PickupArea = PickupArea.Create(this);
        Game = game;

        AddChild(Inventory);
        AddChild(ActionController);
        AddChild(Crafting);
        AddChild(PickupArea);

        PlayerEquipment.InitAsLocal(this);

        Game.InputManager.HorizontalInputChanged += OnHorizontalInputChanged;
        Game.InputManager.JumpPressed += OnJumpPressed;
        Game.Interface.GameMenu.ExitGameButtonDown += OnExitClicked;

        _characterName = playerData["Name"].ToString();

        TreeExiting += () => {
            Game.InputManager.HorizontalInputChanged -= OnHorizontalInputChanged;
            Game.InputManager.JumpPressed -= OnJumpPressed;
            Game.Interface.GameMenu.ExitGameButtonDown -= OnExitClicked;
        };

        LocalPlayerSpawned?.Invoke(this);
    }

    private void OnExitClicked() {
        _camera.Enabled = false;
        Dictionary playerData = new() {
            { "Name", _characterName },
            { "Inventory", Inventory.ToDictionary() },
        };
        FileManager.SavePlayer(playerData);
        PlayerSaved?.Invoke();
    }

    private void OnHorizontalInputChanged(int newInput) {
        _horizontalInput = newInput;
    }

    private void OnJumpPressed() {
        if (_isFalling) return;
        _isFalling = true;
        _yVelocity = -_jumpStrength;
    }

    public override void _PhysicsProcess(double delta) {
        if (!_isLocalPlayer) return;

        Vector2I previousCoords = Coords;
        _isFalling = !TestMove(Transform, new Vector2(0, 0.1f));
        _xVelocity = _speed * _horizontalInput;
        if (_isFalling) {
            _yVelocity += (float)delta * _gravityCoefficient;
        } else {
            _yVelocity = Math.Min(0, _yVelocity);
        }

        Velocity = new Vector2(_xVelocity, _yVelocity);
        MoveAndSlide();

        if (previousCoords == Coords) return;
        MovedCell?.Invoke(Coords, previousCoords);
        
    }

    public void Disable() {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }

    public void Enable() {
        ProcessMode = ProcessModeEnum.Inherit;
        Visible = true;
    }
}