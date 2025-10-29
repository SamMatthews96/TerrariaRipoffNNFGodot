using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Player : CharacterBody2D {
    public static Player Create(int peerId, IntVector spawnCoords) {
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
    public IntVector SpawnCoords { get; private set; }


    public Game Game { get; private set; }
    private int _horizontalInput;
    private bool _isFalling;
    private float _xVelocity;
    private float _yVelocity;
    private string _characterName;

    private IntVector _previousCoords;

    public IntVector Coords => new(Position / Game.BlockSize);

    public int PeerId => Name.ToString().ToInt();
    public bool IsLocalPlayer => Multiplayer.GetUniqueId() == PeerId;


    public static event Action<Player> LocalPlayerSpawned;
    public event Action<Dictionary> MovedCell;

    public event Action<Player> PlayerDespawned;

    public override void _EnterTree() {
        _positionSynchronizer.SetMultiplayerAuthority(PeerId);
        if (IsLocalPlayer) {
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

        _characterName = playerData["Name"].ToString();

        TreeExiting += () => {
            Game.InputManager.HorizontalInputChanged -= OnHorizontalInputChanged;
            Game.InputManager.JumpPressed -= OnJumpPressed;
            SavePlayerData();
        };
        
        LocalPlayerSpawned?.Invoke(this);
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

    private void SavePlayerData() {
        Dictionary playerData = new() {
            {"Name", _characterName},
            { "Inventory", Inventory.ToDictionary() },
        };
        FileManager.SavePlayer(playerData);
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