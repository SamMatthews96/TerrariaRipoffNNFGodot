using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    public static Player Create(World world, int peerId, Vector2I spawnCoords, Dictionary playerData = null) {
        Player player = Data.PackedScenes.Player.Instantiate<Player>();
        player.World = world;
        player.PeerId = peerId;
        player.Name = peerId.ToString();
        player.SpawnCoords = spawnCoords;
        player.SpawnPosition = spawnCoords * Game.BlockSize;
        player.PlayerData = playerData;
        GD.Print(peerId, world.IsHost);
        GD.Print(player.PlayerData);
        
        return player;
    }

    [Export] public Inventory Inventory { get; private set; }
    [Export] public ActionController ActionController { get; private set; }
    [Export] public Crafting Crafting { get; private set; }
    [Export] public PlayerEquipment PlayerEquipment { get; private set; }
    public ServerPickupArea ServerPickupArea { get; private set; }

    [Export] private MultiplayerSynchronizer _positionSynchronizer;
    [Export] private Camera2D _camera;
    [Export] private float _speed = 300f;
    [Export] private float _gravityCoefficient = 1600;
    [Export] private float _jumpStrength = 800;
    [Export] public Vector2 SpawnPosition { get; private set; }
    public Vector2I SpawnCoords { get; private set; }

    public World World { get; private set; }
    public int PeerId { get; private set; }
    
    private int _horizontalInput;
    private bool _isFalling;
    private float _xVelocity;
    private float _yVelocity;
    private string _characterName;
    
    public Dictionary PlayerData { get; private set; }

    public Vector2I Coords => (Vector2I)(Position / Game.BlockSize);

    public bool IsLocalPlayer { get; private set; }

    public static event Action<Player> LocalPlayerSpawned;
    public delegate void CellMovedDelegate(Vector2I newCoords, Vector2I oldCoords);
    public event CellMovedDelegate LocalPlayerMovedCell;
    public event Action<Player> PlayerDespawned;

    public override void _EnterTree() {
        int peerId = Name.ToString().ToInt();
        _positionSynchronizer.SetMultiplayerAuthority(peerId);
        IsLocalPlayer = Multiplayer.GetUniqueId() == peerId;
        if (IsLocalPlayer) {
            _camera.Enabled = true;
        }

        if (World.IsHost) {
            ServerPickupArea = ServerPickupArea.Create(this);
            AddChild(ServerPickupArea);
        }

        if ((World.IsHost || IsLocalPlayer) && PlayerData == null) {
            // error is happening on host
            throw new Exception("PlayerData is null");
        }
    }

    public override void _Ready() {
        Position = SpawnPosition;
        foreach (int peer in Multiplayer.GetPeers()) {
            _positionSynchronizer.SetVisibilityFor(peer, true);
        }

        if (!IsLocalPlayer) return;
        
        PlayerEquipment.InitAsLocal(this);

        World.InputManager.HorizontalInputChanged += OnHorizontalInputChanged;
        World.InputManager.JumpPressed += OnJumpPressed;
        World.Interface.GameMenu.ExitGameButtonDown += OnExitClicked;

        _characterName = PlayerData["Name"].ToString();

        TreeExiting += () => {
            World.InputManager.HorizontalInputChanged -= OnHorizontalInputChanged;
            World.InputManager.JumpPressed -= OnJumpPressed;
            World.Interface.GameMenu.ExitGameButtonDown -= OnExitClicked;
        };
        LocalPlayerSpawned?.Invoke(this);
    }

    public void AddPeerToSynchronizer(int peerId) {
        _positionSynchronizer.SetVisibilityFor(peerId, true);
    }

    public override void _ExitTree() {
        PlayerDespawned?.Invoke(this);
    }

    private void OnExitClicked() {
        _camera.Enabled = false;
        Dictionary playerData = new() {
            { "Name", _characterName },
            { "Inventory", Inventory.ToDictionary() },
        };
        FileManager.SavePlayer(playerData);
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
        if (!IsLocalPlayer) return;

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
        LocalPlayerMovedCell?.Invoke(Coords, previousCoords);
    }
}