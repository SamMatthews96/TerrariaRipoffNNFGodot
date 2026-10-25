using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF;

public partial class World : Node2D {
    public Game Game { get; private set; }
    public Vector2I WorldSize { get; private set; }
    public bool IsHost { get; private set; }
    public Vector2I DefaultSpawnPosition { get; private set; } = new(4, 14);
    [Export] public PickupManager PickupManager { get; private set; }
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public PropManager PropManager { get; private set; }
    [Export] public InputManager InputManager { get; private set; }
    [Export] public Interface.Game Interface { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public ItemIdBimap ItemIdBimap { get; private set; }
    [Export] public StationManager StationManager { get; private set; }
    [Export] public TriggerManager TriggerManager { get; private set; }
    [Export] public NpcManager NpcManager { get; private set; }

    public event Action GameLoaded;

    // Event wrappers to reduce Law of Demeter violations
    public event Action UnequipWeaponClicked {
        add => Interface.PlayerEquipment.UnequipWeaponClicked += value;
        remove => Interface.PlayerEquipment.UnequipWeaponClicked -= value;
    }

    public event Action UnequipPickaxeClicked {
        add => Interface.PlayerEquipment.UnequipPickaxeClicked += value;
        remove => Interface.PlayerEquipment.UnequipPickaxeClicked -= value;
    }

    // Property wrapper for CraftingInterface
    public Interface.Crafting CraftingInterface => Interface.CraftingInterface;

    private Dictionary _localPlayerData;
    public Dictionary WorldData { get; private set; }

    public static World CreateAsHost(Game game, Dictionary worldData, Dictionary playerData) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.IsHost = true;
        world.WorldSize = new Vector2I((int)worldData["Width"], (int)worldData["Height"]);
        world.Game = game;
        world._localPlayerData = playerData;
        world.WorldData = worldData;

        return world;
    }

    public static World CreateAsClient(Dictionary metadata, Dictionary playerData, Game game) {
        World world = Data.PackedScenes.World.Instantiate<World>();
        world.WorldSize = new Vector2I((int)metadata["Width"], (int)metadata["Height"]);
        world.Game = game;
        world.WorldData = metadata;
        world._localPlayerData = playerData;
        return world;
    }

    #region public utils

    public bool IsInBounds(Vector2I coords) {
        return coords.X >= 0
               && coords.X < WorldSize.X
               && coords.Y >= 0
               && coords.Y < WorldSize.Y;
    }

    public bool IsCellFilled(Vector2I coords) {
        if (BlockManager.GetBlock(coords) is not null) return true;
        return PropManager.IsPropAt(coords);
    }

    // @todo this shouldn't check for in bounds
    public bool IsInOrthogonalRange(Vector2I a, Vector2I b, int range) {
        if (!IsInBounds(a) || !IsInBounds(b)) return false;
        if (Math.Abs(a.X - b.X) > range) return false;
        if (Math.Abs(a.Y - b.Y) > range) return false;
        return true;
    }

    public Array<Vector2I> GetCellsInRange(Vector2I coords, int range) {
        int startX = Math.Max(coords.X - range, 0);
        int endX = Math.Min(coords.X + range, WorldSize.X - 1);
        int startY = Math.Max(coords.Y - range, 0);
        int endY = Math.Min(coords.Y + range, WorldSize.Y - 1);

        Array<Vector2I> cells = new();
        for (int x = startX; x <= endX; x++) {
            for (int y = startY; y <= endY; y++) {
                cells.Add(new Vector2I(x, y));
            }
        }
        return cells;
    }

    public CellEntity GetPriorityCellEntity(
        Vector2I coords, Array<CellEntity> types) {
        foreach (CellEntity type in types) {
            switch (type) {
                case CellEntity.Block:
                    if (BlockManager.GetBlock(coords) is not null) {
                        return type;
                    }

                    break;
                case CellEntity.Prop:
                    if (PropManager.IsPropAt(coords)) {
                        return type;
                    }

                    break;
                case CellEntity.Wall:
                    if (BlockManager.GetBlock(coords) is not null) {
                        return type;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return CellEntity.None;
    }

    public Array<Vector2I> GetNewCellsInRange(
        Vector2I newCoords, Vector2I oldCoords, int range
    ) {
        Array<Vector2I> cells = new();

        int minX = Math.Min(newCoords.X, oldCoords.X) - range;
        int maxX = Math.Max(newCoords.X, oldCoords.X) + range;
        int minY = Math.Min(newCoords.Y, oldCoords.Y) - range;
        int maxY = Math.Max(newCoords.Y, oldCoords.Y) + range;

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                Vector2I coords = new(x, y);
                if (!IsInOrthogonalRange(newCoords, coords, range)) continue;
                if (IsInOrthogonalRange(oldCoords, coords, range)) continue;
                cells.Add(coords);
            }
        }

        return cells;
    }

    #endregion

    private void ClientOnSyncComplete() {
        PlayerManager.SpawnPlayersOnClient(_localPlayerData);
        _localPlayerData = null;
        BlockManager.SyncComplete -= ClientOnSyncComplete;
        Show();
        GameLoaded?.Invoke();
    }

    private void OnExitGameClicked() {
        Visible = false;
    }

    public override void _Ready() {
        Interface.GameMenu.ExitGameButtonDown += OnExitGameClicked;
        TreeExiting += () => { Interface.GameMenu.ExitGameButtonDown -= OnExitGameClicked; };
        if (IsHost) {
            PlayerManager.SpawnHostPlayer(_localPlayerData);
            _localPlayerData = null;
            GameLoaded?.Invoke();
        } else {
            Hide();
            BlockManager.SyncComplete += ClientOnSyncComplete;
            BlockManager.ClientGetWorldData();
        }
    }
}